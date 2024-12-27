'use client'
import { useBidStore } from '@/hooks/useBidStore';
import { usePathname } from 'next/navigation';
import React from 'react'
import Countdown, { zeroPad } from 'react-countdown';
type Props ={
    auctionEnd: String;
}
const renderer = ({ days, hours, minutes, seconds, completed }:{days: number,hours: number , minutes: number, seconds: number, completed: boolean}) => {
    return (
        <div className={`
            border-2 bolder-white text-white py-1 px-2 rounded-lg flex justify-center
            ${completed ? 'bg-red-600' : (days ===0 && hours <10) ? 'gb-amber-600' : 'bg-green-600'}
        `}>
            {completed? (<span>Auction Finished</span>): (<span suppressHydrationWarning={true}>{zeroPad(days)}:{zeroPad(hours)}:{zeroPad(minutes)}:{zeroPad(seconds)}</span>)}
        </div>
    )
    if (completed) {
      // Render a completed state
      return <span>Finished</span>
    } else {
      // Render a countdown
      return <span>{days}:{hours}:{minutes}:{seconds}</span>;
    }
  };
export default function CountdownTimer({auctionEnd} :Props) {
  const setOpen = useBidStore(state => state.setOpen);
  const pathname = usePathname();
  function auctionFinished(){
    if (pathname.startsWith('/auctions/details')){
      setOpen(false);
    }
  }
  return (
    <div>
        <Countdown date={auctionEnd.toString()} renderer={renderer} onComplete={auctionFinished}/>
    </div>
  )
}
