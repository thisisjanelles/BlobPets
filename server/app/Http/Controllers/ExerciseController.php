<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-02-23
 * Time: 6:18 PM
 */

namespace App\Http\Controllers;

use App\ExerciseRecord;
use App\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Carbon\Carbon;
use \Response;

class ExerciseController extends Controller
{
    public function __construct()
    {
        $this->middleware('jwt.auth', ['except' => ['createExerciseRecord', 'getExerciseRecord', 'updateExerciseRecord']]);
    }

    /**
     * Creates an exercise record if the user does not already own one
     * @return JsonResponse
     */
    public function createExerciseRecord()
    {
        $ret = $this->verifyUser();
        if(is_int($ret)) {
            $user = $ret;
            $results = ExerciseRecord::where('owner_id', $user)->first();
            // Check if user already has a record
            if (!$results) {
                // Create and return RecordID
                $record = ExerciseRecord::create(array('owner_id' => $user));
                $id = $record->id;
                $user = User::where('id', $user)->first();
                $user->er_id = $id;
                $user->save();
                return response()->json(['ExerciseRecordID' => $id], 201);
            }
            else{
                return response()->json(['error' => 'Existing record found for user'], 400);
            }
        }
        else{
            return $ret;
        }
    }

    /**
     * Gets the exercise record for a user
     * @param $id
     * @return JsonResponse|ExerciseRecord
     */
    public function getExerciseRecord($id)
    {
        $ret = $this->verifyUser();
        if(is_int($ret)) {
            $record = ExerciseRecord::find($id);
            if ($record) {
                return $record;
            }
            else{
                return response()->json(['error' => 'Invalid ExerciseRecordID'], 400);
            }
        }
        else{
            return $ret;
        }
    }

    /**
     * Updates the exercise record for a user
     * @param Request $request
     * @param $id
     * @return JsonResponse|\Illuminate\Http\Response
     */
    public function updateExerciseRecord(Request $request, $id)
    {
        if ($request->exists('distance')) {
            $ret = $this->verifyUser();
            if (is_int($ret)) {
                $user = $ret;
                $record = $this->getExerciseRecord($id);
                // Check that the record exists and that the token owner owns it
                if (is_a($record,'App\ExerciseRecord') and $record->getAttribute('owner_id')== $user) {
                    $distance = $request->input('distance');

                    // Check if it has been a week and the max_exercise needs to be updated
                    $record->updateRecord(Carbon::now());

                    // Update the record
                    $totaldistance = $record->getAttribute('total_exercise');
                    $goaldistance = $record->getAttribute('weekly_goal');
                    $newdistance = $totaldistance + $distance;
                    if($newdistance > $goaldistance){
                        $remaining = 0;
                    }
                    else{
                        $remaining = $goaldistance -$newdistance;
                    }
                    $record->setAttribute('total_exercise', $newdistance);
                    $record->setAttribute('remaining_exercise', $remaining);
                    $record->save();

                    // Update user exercise level
                    $uc = new UserController();
                    $blobs = $uc->getUserBlobs($user);
                    foreach($blobs as $blob) {
                        $old_exercise = $blob->exercise_level;
                        $new_exercise = $old_exercise + $distance;
                        $blob->exercise_level = $new_exercise;
                        $blob->save();
                    }
                    return Response::make('OK', 200);
                    }
                else{
                    return response()->json(['error' => 'Invalid ExerciseRecordID'], 400);
                }
            } else {
                return $ret;
            }
        } else{
            return response()->json(['error' => 'Did not have all required inputs'], 400);
        }
    }

}