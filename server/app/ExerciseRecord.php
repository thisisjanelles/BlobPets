<?php

namespace App;

use App\Http\Controllers\UserController;
use Illuminate\Database\Eloquent\Model;
use \DateTime;
use Carbon\Carbon;

class ExerciseRecord extends Model
{
    //
    protected $guarded = array();

    /**
     * Updates the max_exercise record of the user if it is a new week
     * Also decrements the exercise_level of the user if they haven't met their weekly total
     * @param Carbon $now - time to calculate the record
     */
    public function updateRecord(Carbon $now){
        $km_per_week = 5;
        $penalty = 10;
        $last_update = $this->updated_at;
        $diff = $last_update->diffInDays($now);
        $day_of_week = $last_update->dayOfWeek;
        if ($day_of_week == 0){
            // this is sunday
            $days_left = $day_of_week;
        }
        else{
            //rest of the week
            $days_left = 7 - $day_of_week;
        }
        // New week
        if($days_left < $diff){
            $prevmax = $this->weekly_goal;
            $current_total = $this->total_exercise;


            // Decrement exercise level
            if ($current_total < $prevmax){
                $user_id = $this->owner_id;
                $uc = new UserController();
                $blobs = $uc->getUserBlobs($user_id);
                foreach($blobs as $blob){
                    $old_exercise = $blob->exercise_level;
                    $new_exercise = $old_exercise - $penalty;
                    $blob->exercise_level = $new_exercise;
                    $blob->save();
                }
            }

            // Update Max
            $newmax = $prevmax + $km_per_week;
            $this->weekly_goal = $newmax;
            $this->save();
        }
        else{
            return;
        }
    }

}
