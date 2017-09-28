<?php

namespace Tests\Unit;

use Tests\TestCase;
use Illuminate\Foundation\Testing\DatabaseMigrations;
use Illuminate\Foundation\Testing\DatabaseTransactions;

class UserTest extends TestCase
{
    /**
     * A basic test example.
     *
     * @return void
     */
    public function testExample()
    {
        $this->assertTrue(true);
    }

    protected function setUp()
    {
        parent::setUp();
        $this->artisan("migrate:refresh");
        $this->artisan("db:seed");
    }

    public function tearDown()
    {
        parent::tearDown();
    }

    public function testUserLoginForExistingUser()
    {
    	$response = $this->call('POST', '/api/users/authenticate', array('email' => 'ryanchenkie@gmail.com', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;
        $this->assertEquals(1, $user_id);

        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'chris@scotch.io', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;
        $this->assertEquals(2, $user_id);

        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'holly@scotch.io', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;
        $this->assertEquals(3, $user_id);

        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'adnan@scotch.io', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;
        $this->assertEquals(4, $user_id);
    }

    public function testUserLoginForNonExistingUser()
    {
    	$response = $this->call('POST', '/api/users/authenticate', array('email' => 'some_email@gmail.com', 'password' => 'blah'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $error = $response_json->error;
        $this->assertEquals($error, 'invalid_credentials');
    }

    public function testGetAllUser()
    {
        $response = $this->call('GET', '/api/users/');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
    }

    public function testGetUser()
    {
        $response = $this->call('GET', '/api/users/1');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $name = $response_json->name;
        $this->assertEquals('Ryan Chenkie', $name);

        $response = $this->call('GET', '/api/users/2');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $name = $response_json->name;
        $this->assertEquals('Chris Sevilleja', $name);

        $response = $this->call('GET', '/api/users/3');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $name = $response_json->name;
        $this->assertEquals('Holly Lloyd', $name);

        $response = $this->call('GET', '/api/users/4');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $name = $response_json->name;
        $this->assertEquals('Adnan Kukic', $name);
    }

    public function testGetUserWithInvalideId()
    {
        $response = $this->call('GET', '/api/users/-1');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $error = $response_json->error;
        $this->assertEquals($error, 'User ID invalid');
    }

    public function testGetUserBlob()
    {
        $response = $this->call('GET', '/api/users/1/blobs');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
    }

    public function testGetUserBlobWithInvalideId()
    {
        $response = $this->call('GET', '/api/users/-1/blobs');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $error = $response_json->error;
        $this->assertEquals($error, 'User ID invalid');
    }

    public function testCreateUser()
    {
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser1', 'email' => 'testCreateUser1@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $user_id = $response_json;

        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser2', 'email' => 'testCreateUser2@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $new_user_id = $response_json;
        $this->assertEquals($user_id + 1, $new_user_id);
        $user_id = $new_user_id;

        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser3', 'email' => 'testCreateUser3@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $new_user_id = $response_json;
        $this->assertEquals($user_id + 1, $new_user_id);
        $user_id = $new_user_id;
    }

    public function testCreateUserWithLatLong()
    {
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUserWithLatLong1', 'email' => 'testCreateUserWithLatLong1@gmail.com', 'password' => 'test', 'lat' => 10, 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $user_id = $response_json;

        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUserWithLatLong2', 'email' => 'testCreateUserWithLatLong2@gmail.com', 'password' => 'test', 'lat' => 10, 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $new_user_id = $response_json;
        $this->assertEquals($user_id + 1, $new_user_id);
        $user_id = $new_user_id;

        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUserWithLatLong3', 'email' => 'testCreateUserWithLatLong3@gmail.com', 'password' => 'test', 'lat' => 10, 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $new_user_id = $response_json;
        $this->assertEquals($user_id + 1, $new_user_id);
        $user_id = $new_user_id;
    }

    public function testCreateUserAddressTaken()
    {
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser1', 'email' => 'ryanchenkie@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $user_id = $response_json;
        $error = $response_json->error;
        $this->assertEquals($error, 'Email address has already been taken');
    }

    public function testCreateUserMissingInput()
    {
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser1', 'email' => 'testCreateUser1@gmail.com'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $user_id = $response_json;
        $error = $response_json->error;
        $this->assertEquals($error, 'Missing required input fields');

        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser1', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $user_id = $response_json;
        $error = $response_json->error;
        $this->assertEquals($error, 'Missing required input fields');

        $response = $this->call('POST', '/api/users', array('email' => 'testCreateUser1@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $user_id = $response_json;
        $error = $response_json->error;
        $this->assertEquals($error, 'Missing required input fields');
    }
    
    public function testUpdateUser()
    {
        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'ryanchenkie@gmail.com', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;


        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'name' => 'changeName1'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'password' => 'changePassword1'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'lat' => '10', 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());


        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'name' => 'changeName1', 'password' => 'changePassword1'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'name' => 'changeName1',  'lat' => '10', 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $response = $this->call('PUT', '/api/users/'.$user_id, array('token' => $user_token, 'password' => 'changePassword1',  'lat' => '10', 'long' => 10));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
    }

    public function testUpdateUserUserNotExist()
    {
        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'chris@scotch.io', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $user_token = $response_json->token;
        $user_id = $response_json->id;


        $response = $this->call('PUT', '/api/users/-1', array('token' => $user_token, 'name' => 'changeName1'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
    }

    public function testgetDefendHistory()
    {
        $response = $this->call('GET', '/api/users/1/defendHistory');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $response = $this->call('GET', '/api/users/-1/defendHistory');
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals($response_json->error, 'User ID invalid');
    }




}
