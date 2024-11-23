'use client'
import { useParamsStore } from '@/hooks/useParamsStore'
import { Dropdown, DropdownDivider, DropdownItem } from 'flowbite-react'
import { User } from 'next-auth'
import { signOut } from 'next-auth/react'
import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'

import React from 'react'
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from 'react-icons/ai'
import { HiCog, HiUser } from 'react-icons/hi2'


//pass user object
//'signout' has client side(next-auth/react) and server side(@/auth)
//signout for client is depreciate, changed to redirectTo
type Props={
  user:User
}
export default function UserActions({user}:Props) {
  //redirecting user here
  const router = useRouter()
  const pathname = usePathname()
  const setParams = useParamsStore(state => state.setParam);

  function setWinner(){
    user.email
    setParams({winner: user.username, seller: undefined})
    if (pathname !=='/') router.push('/')// move to root
  }
  function setSeller(){
    setParams({seller: user.username, winner: undefined})
    if (pathname !=='/') router.push('/')// move to root
  }
  return (
    <Dropdown inline label={`welcome ${user.name}`}>
      <DropdownItem icon={HiUser} onClick={setSeller}>
          My Auctions
      </DropdownItem>
      <DropdownItem icon={AiFillTrophy} onClick={setWinner}>
          Auction won
      </DropdownItem>
      <DropdownItem icon={AiFillCar}>
        <Link href='/auctions/create'>
          Sell my car
        </Link>
      </DropdownItem>
      <DropdownItem icon={HiCog}>
        <Link href='/session'>
          Session (dev only)
        </Link>
      </DropdownItem>
      
      <DropdownDivider/>

      <DropdownItem icon={AiOutlineLogout} onClick={() => signOut({redirectTo:'/'})}>
       Sign out
      </DropdownItem>
    </Dropdown>
  )
}
