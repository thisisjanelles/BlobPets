<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-16
 * Time: 7:13 PM
 */

namespace tests\Unit;

use Tests\TestCase;
use App\Http\Controllers\Controller;


class ControllerTokenTest extends TestCase
{
    public $valid_token = '';

    protected function setUp()
    {
        parent::setUp();
        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'ryanchenkie@gmail.com', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->valid_token = $response_json->token;
        $this->artisan("migrate:refresh");
        $this->artisan("db:seed");
    }

    public function tearDown()
    {
        parent::tearDown();
    }

    public function testVerifyUser(){
        $con = new Controller();
        $response = $con->verifyUser();
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $this->assertEquals('The token could not be parsed from the request', $response_json->error);

        //Due to testability reasons, test through another function, no token
        $response = $this->call('POST', '/api/exercises', array());
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $this->assertEquals('The token could not be parsed from the request', $response_json->error);

        //Invalid token
        $this->refreshApplication();
        $response = $this->call('POST','/api/exercises', [], [], [], ['HTTP_Authorization' => 'Bearer' .'asdf']);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $this->assertEquals('Wrong number of segments', $response_json->error);

        $invalid_token = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOjEsImlzcyI6Imh0dHA6XC9cLzEyNy4wLjAuMTo4MDAwXC9hcGlcL3VzZXJzXC9hdXRoZW50aWNhdGUiLCJpYXQiOjE0ODk5NjkyMTMsImV4cCI6MTQ4OTk3MjgxMywibmJmIjoxNDg5OTY5MjEzLCJqdGkiOiIzYTM0MzljZTdmNGE5NDFiNTQzYTViOWY2OTAwMTc5OSJ9.Bp8Qae97d5D6mmwPtBzQ3-DSePXuLsCehxTfunwfPD';

        $this->refreshApplication();
        $response = $this->call('POST','/api/exercises', [], [], [], ['HTTP_Authorization' => 'Bearer' .$invalid_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $this->assertEquals('Token Signature could not be verified.', $response_json->error);
    }

}