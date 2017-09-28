<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use \DateTime;
use Carbon\Carbon;

class Blob extends Model
{

    protected $fillable = ['name', 'type', 'owner_id', 'color'];

    public function updateBlob() {

        $old_time = Carbon::parse($this->last_levels_decrement)->getTimestamp();
        $now = Carbon::now()->getTimestamp();

        // time difference in second
        $timeDifference = $now - $old_time;
        // $year = $timeDifference->y;
        // $month = $timeDifference->m;
        // $day = $timeDifference->d;
        // $hour = $timeDifference->h;
        // $minute = $timeDifference->i;
        // $second = $timeDifference->s;

        // update only if at least 1 minute has passed
        // currently decrease exercise_level, cleanliness_level, and health_level by 1 for each minute passed

        if ($this->alive == false) {
            return;
        }

        // 72 minutes => 5 days = 100 ticks
        $minimumTimeDifferenceInSecond = 4320;

        // 30 seconds for demo purpose
        // $minimumTimeDifferenceInSecond = 30;
        if ($timeDifference >= $minimumTimeDifferenceInSecond) {
            $unitTimePassed = round($timeDifference / $minimumTimeDifferenceInSecond);

            $old_exercise_level = $this->exercise_level;
            $old_cleanliness_level = $this->cleanliness_level;
            $old_health_level = $this->health_level;

            // minimum level is 10 for demo purpose
            $new_exercise_level = max(0, $old_exercise_level - $unitTimePassed);
            $new_cleanliness_level = max(0, $old_cleanliness_level - $unitTimePassed);
            $new_health_level = max(0, $old_health_level - $unitTimePassed);

            $new_blob_levels = array('exercise_level' => $new_exercise_level, 'cleanliness_level' => $new_cleanliness_level, 'health_level' => $new_health_level);
            $this->exercise_level = $new_exercise_level;
            $this->cleanliness_level = $new_cleanliness_level;
            $this->health_level = $new_health_level;
            $this->last_levels_decrement = Carbon::parse($this->last_levels_decrement)->addSeconds($unitTimePassed * $minimumTimeDifferenceInSecond)->toDateTimeString();
            if ($new_health_level <= 0) {
                $this->alive = false;
            }
            $this->save();

        }

    }

}
