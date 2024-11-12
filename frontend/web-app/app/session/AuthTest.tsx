'use client'
import React, { useState } from 'react'
import { updateAuctionTest } from '../actions/auctionActions';
import { Button } from 'flowbite-react';
import { AiFillAccountBook } from 'react-icons/ai';
import { BiLibrary } from 'react-icons/bi';
import { HiUserCircle } from 'react-icons/hi2';

export default function AuthTest() {
    
    const [loading, setLoading] =useState(false);
    const [result, setResult] = useState<any>();
    function doUpdate(){
        setResult(undefined);
        setLoading(true);
        updateAuctionTest()
            .then(res=> setResult(res))
            .catch(err => setResult(err))
            .finally(()=> setLoading(false));
    }
  return (
    <div className='flex  items-center gap-4'>
        <Button  isProcessing={loading} onClick={doUpdate}>
            Test Auth
        </Button>
        <div>
            {JSON.stringify(result,null,2)}
        </div>
    </div>
  )
}
