<?php

use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        $this->call("UserSeed");
        $this->call("BlobSeed");
        $this->call("ExerciseSeed");
    }
}
