<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-27
 * Time: 3:54 PM
 */

namespace tests\Unit;

use App\Http\Controllers\DashboardController;
use Tests\TestCase;
use App\Http\Controllers\BlobController;
use App\Http\Controllers\UserController;

class DashboardTest extends TestCase
{
    public $user_token = '';

    protected function setUp()
    {
        parent::setUp();
        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'ryanchenkie@gmail.com', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->user_token = $response_json->token;
        $this->artisan("migrate:refresh");
        $this->artisan("db:seed");
    }

    public function testGetDashboard(){
        //Invalid
        $this->refreshApplication();
        $response = $this->call('GET','/api/dashboards/', array('type'=>'invalid'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Invalid dashboard type', $response_json->error);

        //Users
        $this->refreshApplication();
        $response = $this->call('GET','/api/dashboards/', array('type'=>'user'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(4, count($response_json));
        $this->assertEquals(0, $response_json[0]->battles_won);
        $this->assertEquals(1, $response_json[0]->id);

        //Blobs
        $response = $this->call('GET','/api/dashboards/', array('type'=>'blob'));
        $dashboard = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $records = $dashboard;
        $this->assertEquals(5, sizeof($records));
        $top1 = $records[0];
        $this->assertEquals(1, $top1->id);
        $top2 = $records[1];
        $this->assertEquals(2, $top2->id);
        $top3 = $records[2];
        $this->assertEquals(3, $top3->id);
        $top4 = $records[3];
        $this->assertEquals(4, $top4->id);
        $top5 = $records[4];
        $this->assertEquals(5, $top5->id);

        //All
        $response = $this->call('GET','/api/dashboards/', array());
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $playerDash = $response_json->userDashboard;
        $blobDash = $response_json->blobDashboard;
        $this->assertEquals(4, sizeof($playerDash));
        $this->assertEquals(5, sizeof($blobDash));

        // Verify user dash
        $topPlayer= $playerDash[0];
        $this->assertEquals(0, $topPlayer->battles_won);
        $this->assertEquals(1, $topPlayer->id);

        $secondPlayer = $playerDash[1];
        $this->assertEquals(0, $secondPlayer->battles_won);
        $this->assertEquals(2, $secondPlayer->id);

        $thirdPlayer = $playerDash[2];
        $this->assertEquals(0, $thirdPlayer->battles_won);
        $this->assertEquals(3, $thirdPlayer->id);

        $lastPlayer = $playerDash[3];
        $this->assertEquals(0, $lastPlayer->battles_won);
        $this->assertEquals(4, $lastPlayer->id);

        // Verify blob dash
        $top1 = $blobDash[0];
        $this->assertEquals(1, $top1->id);
        $top2 = $blobDash[1];
        $this->assertEquals(2, $top2->id);
        $top3 = $blobDash[2];
        $this->assertEquals(3, $top3->id);
        $top4 = $blobDash[3];
        $this->assertEquals(4, $top4->id);
        $top5 = $blobDash[4];
        $this->assertEquals(5, $top5->id);
    }

    public function testGetTopPlayers(){
        // No difference between players
        $dc = new DashboardController();

        $dashboard = $dc->getTopPlayers();
        $this->assertEquals(4, count($dashboard));
        $this->assertEquals(0, $dashboard[0]->battles_won);
        $this->assertEquals(1, $dashboard[0]->id);
        $this->assertEquals(0, $dashboard[1]->battles_won);
        $this->assertEquals(2, $dashboard[1]->id);

        $uc = new UserController();
        $user = $uc->getUser(3);
        $user->battles_won = 60;
        $user->save();

        // One player better than everyone else
        $dashboard = $dc->getTopPlayers();
        $this->assertEquals(4, count($dashboard));
        $topPlayer= $dashboard[0];
        $this->assertEquals(60, $topPlayer->battles_won);
        $this->assertEquals(3, $topPlayer->id);

        // Actual ranking
        $uc = new UserController();
        $user = $uc->getUser(1);
        $user->battles_won = 10;
        $user->save();

        $user = $uc->getUser(4);
        $user->battles_won = 34;
        $user->save();

        $user = $uc->getUser(2);
        $user->battles_won = 5;
        $user->save();

        $this->refreshApplication();
        $dashboard = $dc->getTopPlayers();
        $this->assertEquals(4, count($dashboard));

        $topPlayer= $dashboard[0];
        $this->assertEquals(60, $topPlayer->battles_won);
        $this->assertEquals(3, $topPlayer->id);

        $secondPlayer = $dashboard[1];
        $this->assertEquals(34, $secondPlayer->battles_won);
        $this->assertEquals(4, $secondPlayer->id);

        $thirdPlayer = $dashboard[2];
        $this->assertEquals(10, $thirdPlayer->battles_won);
        $this->assertEquals(1, $thirdPlayer->id);

        $lastPlayer = $dashboard[3];
        $this->assertEquals(5, $lastPlayer->battles_won);
        $this->assertEquals(2, $lastPlayer->id);

        // With 5 players
        $this->refreshApplication();
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser1', 'email' => 'testCreateUser1@gmail.com', 'password' => 'test'));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $fifthUser = $response_json;

        $dashboard = $dc->getTopPlayers();
        $this->assertEquals(5, sizeof($dashboard));

        $topPlayer= $dashboard[0];
        $this->assertEquals(60, $topPlayer->battles_won);
        $this->assertEquals(3, $topPlayer->id);

        $secondPlayer = $dashboard[1];
        $this->assertEquals(34, $secondPlayer->battles_won);
        $this->assertEquals(4, $secondPlayer->id);

        $thirdPlayer = $dashboard[2];
        $this->assertEquals(10, $thirdPlayer->battles_won);
        $this->assertEquals(1, $thirdPlayer->id);

        $fourthPlayer = $dashboard[3];
        $this->assertEquals(5, $fourthPlayer->battles_won);
        $this->assertEquals(2, $fourthPlayer->id);

        $lastPlayer = $dashboard[4];
        $this->assertEquals(0, $lastPlayer->battles_won);
        $this->assertEquals($fifthUser, $lastPlayer->id);

        // With 6 players
        $this->refreshApplication();
        $response = $this->call('POST', '/api/users', array('name' => 'testCreateUser2', 'email' => 'testCreateUser2@gmail.com', 'password' => 'test'));
        json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());

        $dashboard = $dc->getTopPlayers();
        $this->assertEquals(5, count($dashboard));

        $topPlayer= $dashboard[0];
        $this->assertEquals(60, $topPlayer->battles_won);
        $this->assertEquals(3, $topPlayer->id);

        $secondPlayer = $dashboard[1];
        $this->assertEquals(34, $secondPlayer->battles_won);
        $this->assertEquals(4, $secondPlayer->id);

        $thirdPlayer = $dashboard[2];
        $this->assertEquals(10, $thirdPlayer->battles_won);
        $this->assertEquals(1, $thirdPlayer->id);

        $fourthPlayer = $dashboard[3];
        $this->assertEquals(5, $fourthPlayer->battles_won);
        $this->assertEquals(2, $fourthPlayer->id);

        $lastPlayer = $dashboard[4];
        $this->assertEquals(0, $lastPlayer->battles_won);
        $this->assertEquals($fifthUser, $lastPlayer->id);
    }

    public function testGetTopBlobs(){
        $dc = new DashboardController();

        $bc = new BlobController();
        $blob = $bc->getBlob(2);
        $blob->level=60;
        $blob->save();

        $blob = $bc->getBlob(4);
        $blob->level=100;
        $blob->save();

        $blob = $bc->getBlob(5);
        $blob->level=20;
        $blob->save();

        // 5 blobs in system
        $dashboard = $dc->getTopBlobs();

        $records = $dashboard;
        $this->assertEquals(5, sizeof($records));
        $top1 = $records[0];
        $this->assertEquals(4, $top1->id);
        $top2 = $records[1];
        $this->assertEquals(2, $top2->id);
        $top3 = $records[2];
        $this->assertEquals(5, $top3->id);
        $top4 = $records[3];
        $this->assertEquals(1, $top4->id);
        $top5 = $records[4];
        $this->assertEquals(3, $top5->id);

        $this->refreshApplication();
        $response = $this->call('POST','/api/blobs', ['name'=>'testy', 'type'=>'A', 'color'=>'orange'], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $this->assertEquals(6, $response_json->blobID);

        // More than 5 blobs in the system
        $blob = $bc->getBlob(6);
        $blob->level=10;
        $blob->save();

        $dashboard = $dc->getTopBlobs();

        $records = $dashboard;
        $this->assertEquals(5, sizeof($records));
        $top1 = $records[0];
        $this->assertEquals(4, $top1->id);
        $top2 = $records[1];
        $this->assertEquals(2, $top2->id);
        $top3 = $records[2];
        $this->assertEquals(5, $top3->id);
        $top4 = $records[3];
        $this->assertEquals(6, $top4->id);
        $top5 = $records[4];
        $this->assertEquals(1, $top5->id);

        // Less than 5 blobs in system
        $blob = $bc->getBlob(5);
        $blob->alive=false;
        $blob->save();

        $this->refreshApplication();
        $response = $this->call('DELETE','/api/blobs/5', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $blob = $bc->getBlob(1);
        $blob->alive=false;
        $blob->save();

        $this->refreshApplication();
        $response = $this->call('DELETE','/api/blobs/1', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());

        $dashboard = $dc->getTopBlobs();

        $records = $dashboard;
        $this->assertEquals(4, sizeof($records));
        $top1 = $records[0];
        $this->assertEquals(4, $top1->id);
        $top2 = $records[1];
        $this->assertEquals(2, $top2->id);
        $top3 = $records[2];
        $this->assertEquals(6, $top3->id);
        $top4 = $records[3];
        $this->assertEquals(3, $top4->id);
    }

    public function testCalculateLevel(){
        $dc = new DashboardController();

        $this->assertEquals(10, $dc->calculateLevel(10, 100, 100, 100));
        $this->assertEquals(5, $dc->calculateLevel(10, 100, 0, 0));
        $this->assertEquals(3, $dc->calculateLevel(10, 0, 100, 0));
        $this->assertEquals(2, $dc->calculateLevel(10, 0, 0, 100));
        $this->assertEquals(0, $dc->calculateLevel(10, 0, 0, 0));
    }

}