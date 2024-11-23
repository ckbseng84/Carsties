'use client'
import { useParamsStore } from '@/hooks/useParamsStore';
import { usePathname, useRouter } from 'next/navigation';
import React, { useState } from 'react'
import { FaSearch } from 'react-icons/fa'

export default function Search() {
    const router = useRouter();
    const pathname = usePathname();
    const setParam = useParamsStore(state=> state.setParam);
    const setSearchValue = useParamsStore(state=> state.setSearchValue );
    const searchValue = useParamsStore(state=> state.searchValue );

    //search input field changed
    function onChanged(event:any){
        setSearchValue(event.target.value);
    }
    function search(){
        if (pathname !=='/') router.push('/');
        setParam({searchTerm: searchValue});
    }
  return (
    <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
        <input 
            onKeyDown={(e:any)=>{
                if (e.key === 'Enter') search();
            }}
            value={searchValue}
            onChange={onChanged}
            type="text"
            placeholder='Search for cars by make, model and colr'
            className='
                flex-grow
                pl-5
                bg-transparent
                focus:outline-none
                border-transparent
                focus:border-transparent
                focus:ring-0
                text-sm
                text-gray-600
            '
            ></input>
        <button onClick={search}>
            <FaSearch size={34} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2'></FaSearch>
        </button>
        
    </div>
  )
}
