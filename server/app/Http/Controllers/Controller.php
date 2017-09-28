<?php

namespace App\Http\Controllers;

use Illuminate\Foundation\Bus\DispatchesJobs;
use Illuminate\Http\JsonResponse;
use Illuminate\Routing\Controller as BaseController;
use Illuminate\Foundation\Validation\ValidatesRequests;
use Illuminate\Foundation\Auth\Access\AuthorizesRequests;
use Tymon\JWTAuth\Facades\JWTAuth;
use Tymon\JWTAuth\Exceptions\JWTException;
use Tymon\JWTAuth\Exceptions\TokenExpiredException;
use Tymon\JWTAuth\Exceptions\TokenInvalidException;
use Tymon\JWTAuth\Exceptions\TokenBlacklistedException;

class Controller extends BaseController
{
    use AuthorizesRequests, DispatchesJobs, ValidatesRequests;

    /**
     * Verifies that the user token is correct and will return the user id associated with the token if token is valid.
     * If token is invalid or incorrect the function will return a json response with the error code
     * @return JsonResponse or user
     */
    public function verifyUser()
    {
        try {
            $authenticatedUser = JWTAuth::parseToken()->authenticate();
            $user = $authenticatedUser['id'];
            return $user;
        } catch (JwtException $e) {
            $error_code = $e->getMessage();
            return response()->json(['error' => $error_code], 401);
        }
    }
}