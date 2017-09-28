<?php

namespace App\Http\Controllers;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

use Tymon\JWTAuth\Exceptions\JWTException;
use Tymon\JWTAuth\Facades\JWTAuth;
//use Tymon\JWTAuthExceptions\JWTException;
use \App\User;
use \Hash;
use \Response;

class UserController extends Controller
{

    public function __construct()
    {
        $this->middleware('jwt.auth', ['except' => ['authenticate', 'getUsers', 'getUser', 'getUserBlobs', 'getDefendHistory', 'createUser']]);
    }

    /**
     * If no type specified, returns a list of all the users in the database
     * If type, lat, long specified returns a list of nearby users
     * @param Request $request
     * @return User|Collection|JsonResponse|static[]
     */
    public function getUsers(Request $request)
    {
        if ($request->exists('type')){
            $latlong = array('lat', 'long');
            if ($request->exists($latlong)and $request->input('type') == 'nearby'){
                $lat = $request->input('lat');
                $long = $request->input('long');
                return $this->getCloseUsers($lat,$long);
            }
            else{
                return response()->json(['error' => 'Missing required input fields'], 400);
            }
        }
        else{
            $users = User::with("blobs")->get();
            return $users;
        }
    }

    // return the user with the specified user id
    // include the list of blobs associated with the user
    // return error if the user does not exist
    // input:   'id': the id of a user
    public function getUser($id) {
        $user = User::with("blobs")->find($id);
        if (empty($user)) {
            return response()->json(['error' => 'User ID invalid'], 400);
        }
        return $user;
    }

    // return the list of blobs associated with the user with user id
    // return error if the user does not exist
    // input:   'id': the id of a user
    public function getUserBlobs($id) {
        $user = User::find($id);
        if (empty($user)) {
            return response()->json(['error' => 'User ID invalid'], 400);
        }
        return $user->blobs;
    }

    public function getDefendHistory($id) {
        $user = User::find($id);
        if (empty($user)) {
            return response()->json(['error' => 'User ID invalid'], 400);
        }
        return $user->defendHistory;
    }

    // return a token that the user will include for subsequent API calls that require user authentication
    // return error if the credentials provided is invalid or if other errors are encountered
    // input:   'email': the email of the user
    //          'password': the password of the user
    public function authenticate(Request $request)
    {
        $credentials = $request->only('email', 'password');

        // try {
            // verify the credentials and create a token for the user
            if (! $token = JWTAuth::attempt($credentials)) {
                return response()->json(['error' => 'invalid_credentials'], 401);
            }
        // } catch (JWTException $e) {
        //     // something went wrong
        //     return response()->json(['error' => 'could_not_create_token'], 500);
        // }

        // if no errors are encountered we can return a JWT
        $associatedUser = JWTAuth::toUser($token);
        $id = $associatedUser->id;
        return response()->json(compact('token', 'id'));
    }

    // Create a new user with the specified name, email, and password. Return the new user's id if successfully created
    // Return error if some input fields are missing, or the email is already taken by another user
    // input:   'name': the name of the user
    //          'email': the email of the user
    //          'password': the password of the user
    public function createUser(Request $request) {

        $required = array('name', 'email', 'password');
        if ($request->exists($required)) {
            $userEmail = $request->input('email');
            $userWithEmail = User::where('email', $userEmail)->first();
            $userlat = $request->input('lat');
            $userlong = $request->input('long');

            if (empty($userWithEmail)) {
                $credentials = $request->only('name', 'email', 'password');
                if (!empty($userlat) and !empty($userlong)){
                    $credentials['latitude'] = $userlat;
                    $credentials['longitude'] = $userlong;
                }
                $credentials['password'] = Hash::make($credentials['password']);
                $newUser = User::create($credentials);
                return Response::make($newUser->id, 201);
            }
            else {
                return response()->json(['error' => 'Email address has already been taken'], 400);
            }

        }
        else {
            return response()->json(['error' => 'Missing required input fields'], 400);
        }

    }

    // Update an existing user
    // Return error if the user id specified does not exist in the database
    // Return error if the user is trying to modify another user's profile
    // Return 'OK' if the operation was successful
    // input:   'name': the new name for the user profile
    //          'password': the new password for the user profile
    //          'token': the token associated with the logged in user
    public function updateUser($id, Request $request) {

        $token = $request->input('token');
        $newName = $request->input('name');
        $newPassword = $request->input('password');
        $newlat = $request->input('lat');
        $newlong = $request->input('long');

        $associatedUser = JWTAuth::toUser($token);
        $user = User::find($associatedUser->id);
        if ($checkExist = User::find($id)) {
            if ($user->id != $id) {
                return Response::make('Unauthorized', 401);
            }
        }
        else {
            return response()->json(['error' => 'User with specified id does not exist'], 400);
        }

        if (!empty($newName)) {
            $user->name = $newName;
        }
        if (!empty($newPassword)) {
            $user->password = Hash::make($newPassword);
        }
        if (!empty($newlat) and !empty($newlong)){
            $user->latitude = $newlat;
            $user->longitude = $newlong;
        }
        $user->save();

        return Response::make('OK', 200);

    }

    // debug function
    // return the user associated with the token
    // input:   'token': the token of a user
    // public function getTokenOwner(Request $request)
    // {
    //     $token = $request->input('token');

    //     $associatedUser = JWTAuth::toUser($token);

    //     $users = User::all();
    //     return $users->find($associatedUser->id);
    // }

    /**
     * Get users within a boxed area
     * @param $latitude
     * @param $longitude
     * @return mixed(An array of users)
     */
    public function getCloseUsers($latitude, $longitude){
        $box = $this->getBox($latitude,$longitude);
        $users = User::where('latitude', '>=', $box['minLAT'])
            ->where('latitude', '<=', $box['maxLAT'])
            ->where('longitude', '>=', $box['minLON'])
            ->where('longitude', '<=', $box['maxLON'])->with("blobs")->get();

        // Return users that qualify
        return $users;
    }

    /**
     * Checks if two users are close to each other
     * @param $user1
     * @param $user2
     * @return bool
     */
    public function checkCloseUser($user1, $user2){
        $lat = $user1->latitude;
        $long = $user1->longitude;

        $box = $this->getBox($lat, $long);

        $lat2 = $user2->latitude;
        $long2 = $user2->longitude;

        if ($lat2 >= $box['minLAT'] and $lat2 <= $box['maxLAT']){
            if ($long2 >= $box['minLON'] and $long2 <= $box['maxLON']){
                return true;
            }
        }
        return false;
    }

    /**
     * Gives an approximately 1km bounding box, 500m distance from latlong to edge of box
     * @param $latitude
     * @param $longitude
     * @return array
     */
    public function getBox($latitude, $longitude){
        $coord = array();
        $distance = 30;
        $radius = $distance/6371;
        $delta = asin(sin($radius) / cos($latitude));
        $minLat = $latitude - $radius;
        $maxLat = $latitude + $radius;
        $minLon = $longitude - $delta;
        $maxLon = $longitude + $delta;

        $coord['maxLAT'] = round($maxLat, 7);
        $coord['minLAT'] = round($minLat, 7);
        $coord['maxLON'] = round($maxLon, 7);
        $coord['minLON'] = round($minLon, 7);
        return $coord;
    }
}
