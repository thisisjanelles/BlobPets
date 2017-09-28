<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-27
 * Time: 3:27 PM
 */

namespace App\Http\Controllers;

use App\User;
use Illuminate\Http\Request;


class DashboardController extends Controller
{

    public function __construct()
    {
        $this->middleware('jwt.auth', ['except' => ['getDashboard']]);
    }

    /**
     * Gets list of users and blobs for dashboard
     * @param Request $request
     * @return array|\Illuminate\Http\JsonResponse|mixed
     */
    public function getDashboard(Request $request){
        // Takes in type, no need for token
        $dashboard=[];
        if ($request->exists('type')) {
            if ($request->input('type') == 'blob'){
                $dashboard = $this->getTopBlobs();
            }
            else if($request->input('type') == 'user'){
                $dashboard = $this->getTopPlayers();
            }
            else{
                return response()->json(['error' => 'Invalid dashboard type'], 400);
            }
        }
        else{
            //Return all
            $dashboard['userDashboard'] = $this->getTopPlayers();
            $dashboard['blobDashboard'] = $this->getTopBlobs();
        }
        return $dashboard;

    }

    /**
     * Returns list of top 5 players, if less than 5 players in total then all players returned
     * @return mixed
     */
    public function getTopPlayers(){
        $numPlayers = 5;
        $records = User::orderBy('battles_won', 'desc')->take($numPlayers)->get();
        return $records;
    }

    /**
     * Returns list of top 5 blobs, if less than 5 blobs in total then all blobs returned
     * If there is a tie then the top 3 blobs are the oldest ones (has the lowest blobid)
     * @return mixed
     */
    public function getTopBlobs(){
        $numBlobs = 5;
        $blobList = array();
        // get all blobs
        $bc = new BlobController();
        $blobs = $bc->getAllBlobs();
        $totalBlobs = sizeof($blobs);

        // There are less than 5 blobs in the system, we will just return the list of all blobs
        if ($totalBlobs < $numBlobs){
            $numBlobs = $totalBlobs;
        }

        $index = 0;

        // Caculate blob values
        foreach($blobs as $blob){
            $level = $blob->level;
            $exercise = $blob->exercise_level;
            $clean = $blob->cleanliness_level;
            $health = $blob->health_level;
            $value = $this->calculateLevel($level, $health, $exercise, $clean);

            // Add to list of blobs
            $blobList[] = array('id'=> $blob->id, 'value'=>$value);
            $index = $index+1;
        }

        $testing = array();
        foreach ($blobList as $blob){
            $testing[] = $blob['value'];
        }
        // Sort list based on value
        array_multisort($testing, SORT_DESC, $blobList);
        // Return top 5
        $records = array();
        for( $i = 0; $i<$numBlobs; $i++ ) {
            // Access $blobList get blobids and then return them in order
            $blob = $bc->getBlob($blobList[$i]['id']);
            $records[$i] = $blob;
        }
        // return top 5
        return $records;
    }

    /**
     * Calculates the ranking value of the blobs
     * @param $level
     * @param $health_level
     * @param $exercise_level
     * @param $clean_level
     * @return mixed
     */
    public function calculateLevel($level, $health_level, $exercise_level, $clean_level){
        return ($level* ((0.005*$health_level) + (0.003*$exercise_level) +(0.002*$clean_level)));
    }
}