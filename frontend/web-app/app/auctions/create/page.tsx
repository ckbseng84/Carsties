import Heading from '@/app/components/Heading'
import React from 'react'
import AuctionForm from '../AuctionForm'

export default function Create() {
  return (
    <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
      <Heading title={'sell your car'} subtitle={'pleaes enter the detail of your car'} center={false}></Heading>
      <AuctionForm></AuctionForm>
    </div>
  )
}
