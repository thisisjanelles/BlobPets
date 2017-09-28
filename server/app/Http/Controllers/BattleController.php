<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-08
 * Time: 10:20 PM
 */

namespace App\Http\Controllers;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use App\BattleRecord;
use Carbon\Carbon;
use App\Blob;

class BattleController extends Controller
{
    public function __construct()
    {
        $this->middleware('jwt.auth', ['except' => ['createBattleRecord','getBattleRecords', 'getBattleRecord']]);
    }

    /**
     * Creates a battle record
     * @param Request $request
     * @return JsonResponse
     */
    public function createBattleRecord(Request $request){
        $required = array('blob1', 'blob2');
        if ($request->exists($required)){
            //Verify that token in valid
            $ret = $this->verifyUser();
            if(is_int($ret)) {
                $user = $ret;
                $blob1_id = $request->input('blob1');
                $blob2_id = $request->input('blob2');
                $blob1 = Blob::find($blob1_id);
                $blob2 = Blob::find($blob2_id);
                // Check that blobs are valid
                if (!empty($blob1) and !empty($blob2)){
                    $uc = new UserController();
                    $user1_id = $blob1->owner_id;
                    $user2_id = $blob2->owner_id;
                    //Verify that only one of the blobs is owned by them
                    if ($user1_id == $user xor $user2_id == $user) {
                        $user1 = $uc->getUser($user1_id);
                        $user2 = $uc->getUser($user2_id);
                        // Verify that blobs are near each other
                        if ($uc->checkCloseUser($user1, $user2)) {
                            // Verify that blobs are available for battle
                            if ($this->checkRestFlag($blob1) and $this->checkRestFlag($blob2)){
                                // Verify that blobs are fit for battle
                                if ($blob1->health_level > 10 and $blob2->health_level > 10){
                                    //Compute winner
                                    $winner = $this->determineWinner($blob1,$blob2);
                                    if ($winner->id == $blob1_id){
                                        $loser = $blob2;
                                    }
                                    else{
                                        $loser = $blob1;
                                    }
                                    $this->rewardWinner($winner);
                                    $this->updateWinner($winner->owner_id);
                                    $this->punishLoser($loser);

                                    $attacker = $user;
                                    $defender = $user;
                                    if ($user1_id == $user) {
                                        $defender = $user2_id;
                                    } else {
                                        $defender = $user1_id;
                                    }

                                    //Return battle record
                                    $record = BattleRecord::create(array('loserBlobID' => $loser->id, 'winnerBlobID' => $winner->id, 'attackerUserID' => $attacker, 'defenderUserID' => $defender));
                                    $id = $record->id;

                                    //Update rest flags if needed
                                    $this->checkBattleRecord($blob1);
                                    $this->checkBattleRecord($blob2);
                                    return response()->json(['BattleRecordID' => $id], 201);
                                } else{
                                    return response()->json(['error' => 'Blob(s) are unfit for battle'], 400);
                                }
                            } else{
                                return response()->json(['error' => 'Blob(s) are resting'], 400);
                            }
                        } else {
                            return response()->json(['error' => 'Blob(s) are not near each other'], 400);
                        }
                    } else{
                        return response()->json(['error' => 'Invalid blobs, either own both or none'], 400);
                    }
                } else{
                    return response()->json(['error' => 'Blob(s) are invalid'], 400);
                }
            } else{
                return $ret;
            }
        } else{
            return response()->json(['error' => 'Missing required input fields'], 400);
        }
    }

    /**
     * Gets a list of battle records for a specific user, a specific blob or all existing records
     * @param Request $request
     * @return Collection|static[]
     */
    public function getBattleRecords(Request $request){
        $blob = $request->input('blob');
        $user = $request->input('user');
        if (!empty($blob)){
            // Gets the records that include a blob
            $records = BattleRecord::where('winnerBlobID', $blob)->orWhere('loserBlobID', $blob)->get();
            return $records;
        }
        elseif (!empty($user)){
            //Returns all battle records related to a user
            $uc = new UserController();
            $blobs = $uc->getUserBlobs($user);
            $records = new Collection();
            foreach ($blobs as $blob) {
                $blobid = $blob->id;
                $record = BattleRecord::where('winnerBlobID', $blobid)->orWhere('loserBlobID', $blobid)->get();
                $records->push($record);
            }
            return $records;
        }
        else{
            //Returns all records
            $records = BattleRecord::all();
            return $records;
        }
    }

    /**
     * Gets a specific battle record
     * @param $id - the battleRecordID
     * @return JsonResponse|BattleRecord
     */
    public function getBattleRecord($id){
        $record = BattleRecord::where('id', $id)->first();
        // Check that a record exists
        if($record) {
            // return the record
            return $record;
        } else{
            return response()->json(['error' => 'Record does not exist'], 400);
        }
    }
    
    /**
     * Determines the winner of the battle
     * @param $blob1
     * @param $blob2
     * @return Blob
     */
    public function determineWinner($blob1, $blob2){
        // Calculate values
        $exercise = $blob1->exercise_level;
        $health = $blob1->health_level;
        $clean = $blob1->cleanliness_level;
        $level = $blob1->level;

        $blob1_value = $level * ((0.005*$exercise) + (0.003*$health) +(0.002*$clean));
        $exercise = $blob2->exercise_level;
        $health = $blob2->health_level;
        $clean = $blob2->cleanliness_level;
        $level = $blob2->level;
        $blob2_value = $level * ((0.005*$exercise) + (0.003*$health) +(0.002*$clean));

        // Determine winner
        if ($blob1_value > $blob2_value){
            $winner = $blob1;
        }
        else if ($blob1_value < $blob2_value){
            $winner = $blob2;
        }
        else{
            // The targeted blob wins if it is a tie
            $winner = $blob2;
        }

        // Return winner blob
        return $winner;
    }

    /**
     * Update user record for winner
     * @param $winner - the owner_id of the winning blob
     */
    public function updateWinner($winner){
        $uc = new UserController();
        $winner_record = $uc->getUser($winner);
        $winner_record->battles_won = $winner_record->battles_won + 1;
        $winner_record->save();
    }

    /**
     * Update losing blob with a punishment
     * @param $blob
     */
    public function punishLoser($blob){
        $punish = 1;
        $prevBlobHealth = $blob->health_level;
        $prevBlobClean = $blob->cleanliness_level;
        $prevBlobExercise = $blob->exercise_level;

        $blob->health_level =  $prevBlobHealth - $punish;
        $blob->cleanliness_level = $prevBlobClean - $punish;
        $blob->exercise_level = $prevBlobExercise - $punish;

        $blob->save();
    }

    /**
     * Updates the winning blob with some reward
     * @param $blob - the blob that won the battle
     */
    public function rewardWinner($blob){
        $prevBlobLevel = $blob->level;

        $blob->level = $prevBlobLevel + 1;

        $blob->save();
    }

    /**
     * Checks if the rest flag is active
     * @param $blob
     * @return bool
     */
    public function checkRestFlag($blob){
        $blob_rest = Carbon::parse($blob->end_rest);
        $now = Carbon::now();

        //Checks if rest flag is before current time
        $flag = $blob_rest->lt($now);
        return $flag;
    }

    /**
     * Activates the rest flag if blob has battled 5 or more times in last 10 minutes
     * @param $blob
     */
    public function checkBattleRecord($blob){
        $restPeriod = 30;
        $id = $blob->id;
        $records = BattleRecord::where('loserBlobID', $id)->orWhere('winnerBlobID', $id)->orderBy('created_at', 'desc')->take(5)->get();
        $now = Carbon::now();
        // Verify at least 5 records
        if (count($records) >= 5){
            $mostRecent = $records->last()->created_at;
            // Check timestamp between now and oldest of the records if diff is less than 30 raise flag
            if ($mostRecent->diffInMinutes($now) < 10){
                //Change end_flag
                $timeout = Carbon::create($now->year,$now->month,$now->day,$now->hour,$now->minute+$restPeriod,$now->second);
                $blob->end_rest = $timeout;
                $blob->save();
            }
        }
    }
}