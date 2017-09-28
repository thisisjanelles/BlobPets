<?php

use Illuminate\Support\Facades\Schema;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;
use Carbon\Carbon;

class CreateBlobsTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('blobs', function (Blueprint $table) {
            $table->increments('id');
            $table->string('name');
            $table->string('type');
            $table->string('color');
            $table->boolean('alive')->default(true);
            $table->integer('level')->unsigned()->default(1);
            $table->float('exercise_level')->default(60);
            $table->integer('cleanliness_level')->default(60);
            $table->integer('health_level')->default(60);

            $table->dateTime('next_cleanup_time')->default(DB::raw('CURRENT_TIMESTAMP'));
            $table->dateTime('next_feed_time')->default(DB::raw('CURRENT_TIMESTAMP'));
            $table->dateTime('last_levels_decrement')->default(DB::raw('CURRENT_TIMESTAMP'));
            $table->dateTime('end_rest')->default(DB::raw('CURRENT_TIMESTAMP'));

            // $table->dateTime('next_cleanup_time')->default(date("Y-m-d H:i:s"));
            // $table->dateTime('next_cleanup_time')->default(Carbon::now());
            // $table->dateTime('next_feed_time')->default(date("Y-m-d H:i:s"));
            // $table->dateTime('next_feed_time')->default(Carbon::now());
            // $table->dateTime('last_levels_decrement')->default(date("Y-m-d H:i:s"));
            // $table->dateTime('last_levels_decrement')->default(Carbon::now());
            // $table->dateTime('end_rest')->default(date("Y-m-d H:i:s"));
            // $table->dateTime('end_rest')->default(Carbon::now());

            $table->integer('owner_id')->unsigned();
            $table->timestamps();
        });

        Schema::table('blobs', function (Blueprint $table) {
            $table->foreign('owner_id')->references('id')->on('users')->onDelete('cascade');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('blobs');
    }
}
