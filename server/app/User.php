<?php

namespace App;

use Illuminate\Notifications\Notifiable;
use Illuminate\Foundation\Auth\User as Authenticatable;

class User extends Authenticatable
{
    use Notifiable;

    /**
     * The attributes that are mass assignable.
     *
     * @var array
     */
    protected $fillable = [
        'name', 'email', 'password', 'latitude', 'longitude',
    ];

    /**
     * The attributes that should be hidden for arrays.
     *
     * @var array
     */
    protected $hidden = [
        'password', 'remember_token',
    ];

    public function blobs()
    {
        // return $this->hasMany("App\Blob");
        //return $this->hasMany("App\Blob", "owner_id", "id")->select('id', 'name');
        return $this->hasMany("App\Blob", "owner_id");
    }

    public function defendHistory()
    {
        // return $this->hasMany("App\Blob");
        //return $this->hasMany("App\Blob", "owner_id", "id")->select('id', 'name');
        return $this->hasMany("App\BattleRecord", "defenderUserID");
    }

}
