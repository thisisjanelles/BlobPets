<?php
/**
 * Created by PhpStorm.
 * User: Ty
 * Date: 2017-03-11
 * Time: 11:11 PM
 */

namespace tests\Unit;
use app\BattleRecord;
use Carbon\Carbon;
use Tests\TestCase;
use App\Http\Controllers\BattleController;
use App\Http\Controllers\BlobController;
use App\Http\Controllers\UserController;


class BattleTest extends TestCase
{
//|   POST serverAddress.com/api/battles?blob1=<>&blob2=<>
//|       creates a battle record
//|   GET serverAddress.com/api/battles?user=<>
//|   GET serverAddress.com/api/battles?blob=<>
//|   GET serverAddress.com/api/battles
//|       Gets the battles associated with the user, blob or all records
//|   GET serverAddress.com/api/battles/{id}
//|       Get a specific battle record

    public $user_token = '';

    protected function setUp(){
        parent::setUp();
        $response = $this->call('POST', '/api/users/authenticate', array('email' => 'ryanchenkie@gmail.com', 'password' => 'secret'));
        $response_json = json_decode($response->getContent());
        $this->user_token = $response_json->token;
        $this->artisan("migrate:refresh");
        $this->artisan("db:seed");

        $bc = new BlobController();

        $blob1 = $bc->getBlob(1);
        $blob2 = $bc->getBlob(3);

        $blob1->health_level = 60;
        $blob1->end_rest = Carbon::yesterday();
        $blob1->save();

        $blob2->health_level = 60;
        $blob2->end_rest = Carbon::yesterday();
        $blob2->save();

    }

    public function tearDown()
    {
        parent::tearDown();
    }

    public function testCreateBattleRecordErrors(){
        // No token
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', array('blob1'=>1, 'blob2'=>5));
        $response_json = json_decode($response->getContent());
        $this->assertEquals(401, $response->getStatusCode());
        $this->assertEquals('The token could not be parsed from the request', $response_json->error);

        // Missing inputs
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Missing required input fields', $response_json->error);

        // Invalid blob
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>89], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are invalid', $response_json->error);

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>90, 'blob2'=>89], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are invalid', $response_json->error);

        // Own both blobs
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>5], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Invalid blobs, either own both or none', $response_json->error);

        // Own neither blob
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>2, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Invalid blobs, either own both or none', $response_json->error);

        // Blobs are not near each other
        $uc = new UserController();

        $user = $uc->getUser(1);
        $user->latitude = 49.184148163083;
        $user->longitude = -123.11260905865;

        $user->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are not near each other', $response_json->error);
    }

    public function testCreateBattleRecord(){

        $bc = new BlobController();
        $bac = new BattleController();

        // Creates record where initator blob wins
        $blob = $bc->getBlob(5);
        $blob->level = 10;
        $blob->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>5, 'blob2'=>2], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $this->assertEquals(1, $response_json->BattleRecordID);

        $record = $bac->getBattleRecord($response_json->BattleRecordID);
        $this->assertEquals(5, $record->winnerBlobID);
        $this->assertEquals(2, $record->loserBlobID);

        // Creates record where passive blob wins
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>2, 'blob2'=>5], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $this->assertEquals(2, $response_json->BattleRecordID);

        $record = $bac->getBattleRecord($response_json->BattleRecordID);
        $this->assertEquals(5, $record->winnerBlobID);
        $this->assertEquals(2, $record->loserBlobID);

        // Creates record where blobs have the same default stats
        $blob = $bc->getBlob(1);
        $prev_time = $blob->end_rest;
        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(201, $response->getStatusCode());
        $this->assertEquals(3, $response_json->BattleRecordID);

        $record = $bac->getBattleRecord($response_json->BattleRecordID);
        $this->assertEquals(3, $record->winnerBlobID);
        $this->assertEquals(1, $record->loserBlobID);

        $post_time = $blob->end_rest;
        $this->assertSame($prev_time, $post_time);

        // Creates multiple battle record and checks that rest flag is updated
        $prev_time = $blob->end_rest;
        for( $i = 0; $i<5; $i++ ) {
            $this->refreshApplication();
            $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        }

        $blob = $bc->getBlob(1);
        $post_time = $blob->end_rest;
        $this->assertNotEquals($prev_time, $post_time);

    }

    public function testCreateBattleRecord2(){
        // Blob is resting
        $bc = new BlobController();
        $blob1 = $bc->getBlob(1);
        $blob2 = $bc->getBlob(3);

        $blob1->end_rest = Carbon::tomorrow();
        $blob1->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are resting', $response_json->error);

        $blob2->end_rest = Carbon::tomorrow();
        $blob2->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are resting', $response_json->error);

        // Blob is unfit for battle
        $blob1 = $bc->getBlob(1);
        $blob2 = $bc->getBlob(3);

        $blob1->health_level = 5;
        $blob1->end_rest = Carbon::yesterday();
        $blob1->save();


        $blob2->end_rest = Carbon::yesterday();
        $blob2->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are unfit for battle', $response_json->error);

        $blob2 = $bc->getBlob(3);
        $blob2->health_level = 5;
        $blob2->save();

        $this->refreshApplication();
        $response = $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Blob(s) are unfit for battle', $response_json->error);
    }
    public function testGetBattleRecords(){

        $this->refreshApplication();
        $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);

        // Get all records for blob
        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', ['blob'=>1], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0]->loserBlobID);

        // Get all records for user
        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', ['user'=>1], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0][0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0][0]->loserBlobID);

        // Get all Records
        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0]->loserBlobID);

        $this->refreshApplication();
        $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);

        $this->refreshApplication();
        $this->call('POST','/api/battles', ['blob1'=>5, 'blob2'=>4], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);

        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', ['blob'=>1], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0]->loserBlobID);
        $this->assertEquals(3, $response_json[1]->winnerBlobID);
        $this->assertEquals(1, $response_json[1]->loserBlobID);

        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', ['user'=>1], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0][0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0][0]->loserBlobID);
        $this->assertEquals(3, $response_json[0][1]->winnerBlobID);
        $this->assertEquals(1, $response_json[0][1]->loserBlobID);
        $this->assertEquals(4, $response_json[1][0]->winnerBlobID);
        $this->assertEquals(5, $response_json[1][0]->loserBlobID);

        $this->refreshApplication();
        $response = $this->call('GET','/api/battles', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(200, $response->getStatusCode());
        $this->assertEquals(3, $response_json[0]->winnerBlobID);
        $this->assertEquals(1, $response_json[0]->loserBlobID);
        $this->assertEquals(3, $response_json[1]->winnerBlobID);
        $this->assertEquals(1, $response_json[1]->loserBlobID);
        $this->assertEquals(4, $response_json[2]->winnerBlobID);
        $this->assertEquals(5, $response_json[2]->loserBlobID);

    }

    public function testGetBattleRecord(){
        // Record does not exist
        $this->refreshApplication();
        $response = $this->call('GET','/api/battles/1', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(400, $response->getStatusCode());
        $this->assertEquals('Record does not exist', $response_json->error);

        // Record exists
        $this->refreshApplication();
        $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $this->refreshApplication();
        $response = $this->call('GET','/api/battles/1', [], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        $response_json = json_decode($response->getContent());
        $this->assertEquals(3, $response_json->winnerBlobID);
        $this->assertEquals(1, $response_json->loserBlobID);
    }

    public function testDetermineWinner(){
        $bc = new BlobController();
        $blob1 = $bc->getBlob(1);
        $blob2 = $bc->getBlob(3);

        $bac = new BattleController();

        // Blobs are at same level
        $winner = $bac->determineWinner($blob1, $blob2);
        $this->assertEquals(3, $winner->id);

        // Blob 1 is stronger
        $bac->rewardWinner($blob1);
        $winner = $bac->determineWinner($blob1, $blob2);
        $this->assertEquals(1, $winner->id);

        // Blob 3 is stronger
        $blob2 = $bc->getBlob(3);
        $bac->rewardWinner($blob2);

        $blob2 = $bc->getBlob(3);
        $bac->rewardWinner($blob2);

        $blob1 = $bc->getBlob(1);
        $blob2 = $bc->getBlob(3);
        $winner = $bac->determineWinner($blob1, $blob2);
        $this->assertEquals(3, $winner->id);

    }

    public function testUpdateWinner(){
        $uc = new UserController();
        $user = $uc->getUser(1);

        $bac = new BattleController();
        $bac->updateWinner($user);

        $user = $uc->getUser(1);
        $this->assertEquals(1, $user->battles_won);
    }

    public function testPunishLoser(){
        $bc = new BlobController();
        $blob = $bc->getBlob(1);

        $bac = new BattleController();
        $bac->punishLoser($blob);

        $blob = $bc->getBlob(1);
        $this->assertEquals(59, $blob->exercise_level);
        $this->assertEquals(59, $blob->health_level);
        $this->assertEquals(59, $blob->cleanliness_level);
    }

    public function testRewardWinner(){
        $bc = new BlobController();
        $blob = $bc->getBlob(1);

        $bac = new BattleController();
        $bac->rewardWinner($blob);

        $blob = $bc->getBlob(1);
        $this->assertEquals(2, $blob->level);
    }

    public function testCheckRestFlag(){
        $bc = new BlobController();
        $blob = $bc->getBlob(1);

        $bac = new BattleController();
        $this->assertTrue($bac->checkRestFlag($blob));

        $blob->end_rest = Carbon::tomorrow();
        $blob->save();

        $this->assertFalse($bac->checkRestFlag($blob));
    }

    public function testCheckBattleRecord(){
        $bc = new BlobController();
        $blob = $bc->getBlob(1);

        $bac = new BattleController();

        // Blob has less than 5 battles
        $prev_time = $blob->end_rest;
        $bac->checkBattleRecord($blob);
        $post_time = $blob->end_rest;
        $this->assertEquals($prev_time, $post_time);

        // Blob has 5 records and they are all recent
        $prev_time = $blob->end_rest;
        for( $i = 0; $i<6; $i++ ) {
            $this->refreshApplication();
            $this->call('POST','/api/battles', ['blob1'=>1, 'blob2'=>3], [], [], ['HTTP_Authorization' => 'Bearer' . $this->user_token]);
        }
        $bac->checkBattleRecord($blob);
        $post_time = $blob->end_rest;
        $this->assertNotEquals($prev_time, $post_time);

        // Blob has 5 records but the oldest one is a while ago
        $lastRecord = BattleRecord::where('loserBlobID', 1)->orWhere('winnerBlobID', 1)->orderBy('created_at', 'desc')->take(5)->get()->last();
        $lastRecord->created_at = Carbon::yesterday();
        $lastRecord->save();

        $prev_time = $blob->end_rest;
        $bac->checkBattleRecord($blob);

        $post_time = $blob->end_rest;
        $this->assertEquals($prev_time, $post_time);

    }

}