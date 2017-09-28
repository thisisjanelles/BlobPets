<?php

use Illuminate\Database\Seeder;
use Illuminate\Database\Eloquent\Model;

use App\Blob;
use App\BreedingRecord;

use Faker\Factory as Faker;

class BlobSeed extends Seeder
{
	/**
	 * Run the database seeds.
	 *
	 * @return void
	 */
	public function run()
	{
		$blobs = array(
			['name' => 'Blob 1', 'type' => 'type A', 'owner_id' => 1, 'color'=> 'orange'],
			['name' => 'Blob 2', 'type' => 'type A', 'owner_id' => 2, 'color'=>  'blue'],
			['name' => 'Blob 3', 'type' => 'type B', 'owner_id' => 3, 'color'=> 'green'],
			['name' => 'Blob 4', 'type' => 'type B', 'owner_id' => 4, 'color'=> 'pink'],
			['name' => 'Blob 5', 'type' => 'type C', 'owner_id' => 1, 'color'=> 'green'],
		);

		foreach ($blobs as $blob)
        {
            $blobCreated = Blob::create($blob);
            BreedingRecord::create(array('id' => $blobCreated->id));
        }
        
	}
}
