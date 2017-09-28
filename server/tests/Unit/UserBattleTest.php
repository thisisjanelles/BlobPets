<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-15
 * Time: 7:29 PM
 */

namespace tests\Unit;

use Tests\TestCase;
use App\Http\Controllers\UserController;

class UserBattleTest extends TestCase
{

    protected function setUp(){
        parent::setUp();
        $this->artisan("migrate:refresh");
        $this->artisan("db:seed");
    }

    public function testGetUsers(){
        // GET without both lat and long
        $response = $this->call('GET','/api/users/', array());
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(4, count($response_json));

        // GET with just type
        $response = $this->call('GET','/api/users/', array('type'=>'nearby'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Missing required input fields', $response_json->error);

        // GET with just type not nearby
        $response = $this->call('GET','/api/users/', array('type'=>'asdf'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Missing required input fields', $response_json->error);

        // GET with just type and lat or long
        $response = $this->call('GET','/api/users/', array('type'=>'nearby', 'lat'=>0));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Missing required input fields', $response_json->error);

        // GET with both lat and long but no users found
        $response = $this->call('GET','/api/users/', array('type'=>'nearby', 'lat'=> 49.188857, 'long'=> -123.102681));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(0, count($response_json));

        // GET with users found
        $response = $this->call('GET','/api/users/', array('type'=>'nearby', 'lat'=> 0, 'long'=> 0));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(4, count($response_json));

        // Change lat long and verify that only certain users are found
        $uc = new UserController();
        $user = $uc->getUser(1);
        $user->latitude = 49.188857;
        $user->longitude = -123.102681;
        $user->save();

        $response = $this->call('GET','/api/users/', array('type'=>'nearby', 'lat'=> 0, 'long'=> 0));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, count($response_json));

    }

    public function testGetCloseUser(){
        $uc = new UserController();
        // get users within the center of the box
        $users = $uc->getCloseUsers(0,0);
        $this->assertEquals(4, count($users));

        // get users within the box
        $user = $uc->getUser(1);
        $user->latitude = 49.188857;
        $user->longitude = -123.102681;
        $user->save();

        $user = $uc->getUser(2);
        $user->latitude = 49.188860;
        $user->longitude = -123.102630;
        $user->save();

        $users = $uc->getCloseUsers(49.188864,-123.102650);
        $this->assertEquals(2, count($users));

        // Check that edge/boundary users are also returned
        $user = $uc->getUser(1);
        $user->latitude = 49.193565836917;
        $user->longitude = -123.09275294135;
        $user->save();

        $user = $uc->getUser(2);
        $user->latitude = 49.193565836917;
        $user->longitude = -123.11260905865;
        $user->save();

        $user = $uc->getUser(3);
        $user->latitude = 49.184148163083;
        $user->longitude = -123.09275294135;
        $user->save();

        $user = $uc->getUser(4);
        $user->latitude = 49.184148163083;
        $user->longitude = -123.11260905865;
        $user->save();

        $users = $uc->getCloseUsers(49.188857,-123.102681);
        $this->assertEquals(4, count($users));
    }

    public function testCheckCloseUser(){
        $uc = new UserController();

        $user1 = $uc->getUser(1);
        $user2 = $uc->getUser(2);

        // Users are at the same location
        $this->assertTrue($uc->checkCloseUser($user1, $user2));
        $this->assertTrue($uc->checkCloseUser($user2, $user1));

        $user1->latitude = 49.188857;
        $user1->longitude = -123.102681;
        $user1->save();

        $user1 = $uc->getUser(1);

        // Users are not near each other
        $this->assertFalse($uc->checkCloseUser($user1, $user2));
        $this->assertFalse($uc->checkCloseUser($user2, $user1));

        $user2->latitude = 49.184148163083;
        $user2->longitude = -123.11260905865;
        $user2->save();

        $user1 = $uc->getUser(1);
        $user2 = $uc->getUser(2);

        // Users are near each other
        $this->assertTrue($uc->checkCloseUser($user1, $user2));
        $this->assertTrue($uc->checkCloseUser($user2, $user1));
    }


}