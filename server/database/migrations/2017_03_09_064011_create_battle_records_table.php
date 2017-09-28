<?php

use Illuminate\Support\Facades\Schema;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateBattleRecordsTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('battle_records', function (Blueprint $table) {
            $table->increments('id');
            $table->integer('loserBlobID')->unsigned();
            $table->integer('winnerBlobID')->unsigned();
            $table->integer('attackerUserID')->unsigned();
            $table->integer('defenderUserID')->unsigned();
            $table->timestamps();
        });

        Schema::table('battle_records', function (Blueprint $table) {
            $table->foreign('attackerUserID')->references('id')->on('users')->onDelete('cascade');
            $table->foreign('defenderUserID')->references('id')->on('users')->onDelete('cascade');
            
            $table->foreign('winnerBlobID')->references('id')->on('blobs')->onDelete('cascade');
            $table->foreign('loserBlobID')->references('id')->on('blobs')->onDelete('cascade');
        });

//        Schema::table('battle_records', function (Blueprint $table) {
//            $table->foreign('blob_id1')->references('id')->on('blobs')->onDelete('cascade');
//            $table->foreign('blob_id2')->references('id')->on('blobs')->onDelete('cascade');
//        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('battle_records');
    }
}
