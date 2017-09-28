<?php

use Illuminate\Support\Facades\Schema;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateBreedingRecordsTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('breeding_records', function (Blueprint $table) {
            // $table->increments('id');
            $table->integer('id')->unsigned();
            $table->primary('id');
            $table->integer('parent1_id')->unsigned()->nullable();
            $table->integer('parent2_id')->unsigned()->nullable();
            $table->timestamps();
        });

        Schema::table('breeding_records', function (Blueprint $table) {
            $table->foreign('id')->references('id')->on('blobs')->onDelete('cascade');
            $table->foreign('parent1_id')->references('id')->on('blobs');
            $table->foreign('parent2_id')->references('id')->on('blobs');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('breeding_records');
    }
}
