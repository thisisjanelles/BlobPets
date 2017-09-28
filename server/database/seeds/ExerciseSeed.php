<?php

use Illuminate\Database\Seeder;
use App\ExerciseRecord;

class ExerciseSeed extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        //
        $records = array(
            ['owner_id' => 1],
            ['owner_id' => 2],
        );

        foreach ($records as $record)
        {
            $recordCreated = ExerciseRecord::create($record);
        }
    }
}
