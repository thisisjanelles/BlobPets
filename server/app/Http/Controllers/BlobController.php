<?php

namespace App\Http\Controllers;

use App\Blob;
use Illuminate\Http\Request;
use Carbon\Carbon;
use \Response;


class BlobController extends Controller
{
    public function __construct()
    {
        $this->middleware('jwt.auth', ['except' => ['getAllBlobs', 'getBlob', 'getBlobUpdatedAt', 'updateBlob', 'createBlob', 'deleteBlob']]);
    }

    // return a list of all the blobs in the database
    // input:   none
    public function getAllBlobs()
    {
        $blobs = Blob::all();
        foreach ($blobs as $blob)
        {
            $blob->updateBlob();
        }
        return $blobs;
    }

    // return the blob with the specified blob id
    // return error if the blob does not exist
    // input:   'id': the id of a blob
    public function getBlob($id)
    {
        $blob = Blob::find($id);
        if (empty($blob)) {
            return response()->json(['error' => 'Blob ID invalid'], 400);
        }
        $blob->updateBlob();
        return $blob;
    }

    // update the specified blob's name or level attributes
    // return error if the blob does not exist
    // input:   'id': the id of a blob
    //          'name': new name for the blob
    public function updateBlob(Request $request, $id) {
        $ret = $this->verifyUser();
        // $blob = BlobController::getBlob($id);
        $blob = Blob::find($id);
        if(is_int($ret)){
            $user = $ret;
            if (!empty($blob)){
                if ($user == $blob->owner_id) {
                    $blob->updateBlob();

                    $new_name = $request->input('name');
                    // $new_type = $request->input('type', $blob->type);
                    // $new_exercise_level = $request->input('exercise_level');
                    $new_cleanliness_level = $request->input('cleanliness_level');
                    $new_health_level = $request->input('health_level');


                    $rejectRequest = false;

                    if (!empty($new_name)) {
                        $blob->name = $new_name;
                    }
                    // $blob->type = $new_type;
                    // TODO: do request verification and generate a new timestamp for next event
                    //          Old timestamp should be in the past, otherwise, reject request

                    // if (!empty($new_exercise_level)) {
                    //     $blob->exercise_level = $new_exercise_level;
                    // }

                    if (!empty($new_cleanliness_level)) {
                        if (BlobController::timeValueIsInThePast($blob->next_cleanup_time)) {
                            if ($new_cleanliness_level - $blob->cleanliness_level <= 11) {
                                $blob->cleanliness_level = $blob->cleanliness_level + 10;
                                $blob->next_cleanup_time = BlobController::generateNewTime();
                            } else {
                                $rejectRequest = true;
                            }
                        } else {
                            $rejectRequest = true;
                        }
                    }

                    if (!empty($new_health_level)) {
                        if (BlobController::timeValueIsInThePast($blob->next_feed_time)) {
                            if ($new_health_level - $blob->health_level <= 11) {
                                $blob->health_level = $blob->health_level + 10;
                                $blob->next_feed_time = BlobController::generateNewTime();
                            } else {
                                $rejectRequest = true;
                            }

                        } else {
                            $rejectRequest = true;
                        }
                    }

                    if ($rejectRequest == false) {
                        $blob->save();
                    } else {
                        return response()->json(['error' => 'Request rejected'], 403);
                    }

                    return Response::make('OK', 200);
                } else {
                    return response()->json(['error' => 'Unauthorized action'], 401);
                }
            }
            else{
                return response()->json(['error' => 'Blob ID invalid'], 400);
            }
        }
        else {
            return $ret;
        }
    }

    /**
     * Creates a new blob if owner does not already have maxNumBlobs
     * @param Request $request
     * @return \Illuminate\Http\JsonResponse
     */
    public function createBlob(Request $request)
    {
        $maxNumBlobs = 4;
        $expectedCreateNew = array('name', 'type', 'color');
        $expectedBreed = array('id1', 'id2');
        // check for correct number of inputs
        if ($request->exists($expectedCreateNew)) {
            $blobName = $request->input('name');
            $blobType = $request->input('type');
            $blobColor = $request->input('color');

            // check if owner is valid
            $ret = $this->verifyUser();
            if (is_int($ret)) {
                $user = $ret;
                $numBlobs = Blob::where('owner_id', $user)->count();

                // check that user does not have the max number of blobs
                if ($numBlobs<$maxNumBlobs){

                    //check if inputs are valid
                    $supportedTypes = array('A', 'B', 'C');
                    $supportedColors = array('orange', 'blue', 'green', 'pink');
                    if (in_array($blobType, $supportedTypes) and in_array($blobColor, $supportedColors)) {
                        $blob = Blob::create(array('name' => $blobName, 'type' => 'type ' .$blobType, 'owner_id' => $user, 'color' => $blobColor));
                        $id = $blob->id;

                        // return blob id
                        return response()->json(['blobID' => $id], 201);
                    } else{
                        return response()->json(['error' => 'Invalid inputs'], 400);
                    }
                }
                else{
                    return response()->json(['error' => 'User has max number of blobs'], 400);
                }
            }
            else {
                return $ret;
            }
        } else if ($request->exists($expectedBreed)) {

            $parentBlob1 = $request->input('id1');
            $parentBlob2 = $request->input('id2');

            // check if owner is valid
            $ret = $this->verifyUser();
            if (is_int($ret)) {

                if (empty(Blob::find($parentBlob1)) || empty(Blob::find($parentBlob2)) || Blob::find($parentBlob1)->owner_id != $ret || Blob::find($parentBlob2)->owner_id != $ret) {
                    return response()->json(['error' => 'Invalid blob id'], 400);
                }

                $user = $ret;
                $numBlobs = Blob::where('owner_id', $user)->count();

                // check that user does not have the max number of blobs
                if ($numBlobs < $maxNumBlobs){

                    $parentBlob1Color = Blob::find($parentBlob1)->color;
                    $parentBlob2Color = Blob::find($parentBlob2)->color;

                    $blobName = 'Juvenile Blob';
                    $blobType = 'A';
                    $blobColor = $this->mixColor($parentBlob1Color, $parentBlob2Color);
                    $blob = Blob::create(array('name' => $blobName, 'type' => 'type ' .$blobType, 'owner_id' => $user, 'color' => $blobColor));
                    $id = $blob->id;

                    return response()->json(['blobID' => $id], 201);

                }
                else{
                    return response()->json(['error' => 'User has max number of blobs'], 400);
                }
            }
            else {
                return $ret;
            }

        } else {
            return response()->json(['error' => 'Did not have all required inputs'], 400);
        }

    }

    /**
     * Deletes a blob if blob health = 0
     * @param $id - the id of the blob to delete
     * @return \Illuminate\Http\JsonResponse|\Illuminate\Http\Response
     */
    public function deleteBlob($id){
        $ret = $this->verifyUser();
        if(is_int($ret)) {
            $user = $ret;
            // Check if blob exists and that it belongs to user
            $results = Blob::where('id', $id)->where('owner_id',$user)->first();
            if(!empty($results)) {
                // Check if blob health = 0
                if ($results->alive == false){
                    // Delete blob
                    Blob::destroy($id);
                    return Response::make('OK', 200);
                }
                else{
                    return response()->json(['error' => 'Blob is alive'], 400);
                }
            }
            else{
                return response()->json(['error' => 'Invalid blobID'], 400);
            }
        }
        else{
            return $ret;
        }

    }

    // helper function
    public function timeValueIsInThePast($timeValue) {
        $oldTime = Carbon::parse($timeValue)->getTimestamp();
        $now = Carbon::now()->getTimestamp();
        $timeDifference = $now - $oldTime;

        return $timeDifference >= 0;
    }

    // generate a new time for the next event.
    // the next event will be between 6 and 12 hours from now
    public function generateNewTime() {
        // set it to 1 minutes for demo purpose
        $minimumHour = 6;
        $randomHours = rand(0, 5);
        $randomMinutes = rand(0, 59);
        $randomSeconds = rand(0, 59);
        return Carbon::now()->addHours($minimumHour + $randomHours)->addMinutes($randomMinutes)->addSeconds($randomSeconds)->toDateTimeString();
    }

    public function mixColor($color1, $color2)
    {
        $colorInt = 0;
        $colorInt = $colorInt + $this->colorToInt($color1);
        $colorInt = $colorInt + $this->colorToInt($color2);
        $colorInt = $colorInt % 4;
        if ($colorInt == 0) {return 'orange';}
        if ($colorInt == 1) {return 'blue';}
        if ($colorInt == 2) {return 'green';}
        return 'pink';
    }

    public function colorToInt($color)
    {
        if (str_is('orange', $color)) {return 0;}
        if (str_is('blue', $color)) {return 1;}
        if (str_is('green', $color)) {return 2;}
        return 3;
    }

}
