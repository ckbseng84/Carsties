'use client'
import { Button, TextInput } from 'flowbite-react';
import React, { useEffect } from 'react'
import { FieldValues, useForm } from 'react-hook-form'
import Input from '../components/Input';
import DateInput from '../components/DateInput';
import { createAuction, updateAuction } from '../actions/auctionActions';
import { usePathname, useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Auction } from '@/types';

type Props ={
    auction?: Auction
}
export default function AuctionForm({auction}: Props) {
    const router = useRouter();
    const pathname = usePathname();
    const {
        control,
        register, 
        handleSubmit,
        setFocus,
        reset,
        formState: {isSubmitting, isValid}
    } = useForm({
        mode: 'onTouched'//trigger validate on unfocused
    });
    //set focus on make
    useEffect(() =>{
        if (auction) {
            const {make, model, color, mileage, year} = auction;
            reset({make, model, color, mileage, year});
        }
        setFocus('make')
    },[setFocus]);

    async function onSubmit(data: FieldValues){
        try {
            let id = '';
            let res;
            if (pathname === '/auctions/create'){
                res = await createAuction(data);
                id = res.id;
            }else {
                if(auction){
                    res = await updateAuction(data,auction.id);
                    id = auction.id;
                }
            }
            if (res.error){
                throw res.error;
            }
            toast.success('auction created')
            router.push(`/auctions/details/${id}`)// temporary declare id, as id is 'any' object 
        } catch (error:any) {
            toast.error(error.status + ' ' + error.message)
        }
    }

  return (
    <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
        <Input label='Make'  name='make' control={control} rules={{required:'Make is required'}}/>
        <Input label='Model' name='model' control={control} rules={{required:'Model is required'}}/>
        <Input label='Color' name='color' control={control} rules={{required:'Color is required'}}/>
        {/* <div className='mb-3 block'>
        <TextInput {...register('model',{required:'Model is required'})}
                placeholder='model'
                color={errors?.model && 'failure'}//set color to failure(red)
                helperText={errors.model?.message as string}
            ></TextInput>
        </div> */}
        <div className='grid grid-cols-2 gap-2'>
            <Input label='Year' name='year' control={control} rules={{required:'Year is required'}}/>
            <Input label='Mileage' name='mileage' type='number' control={control} rules={{required:'Mileage is required'}}/>

        </div>
        {pathname === '/auction/create' && (
        <>
            <Input label='Image Url' name='imageUrl' control={control} rules={{required:'Image Url is required'}}/>
            <div className='grid grid-cols-2 gap-2'>
                <Input label='Reserve Price (Enter 0 is no reserve)' name='reservicePrice' control={control} rules={{required:'Reserve price is required'}}/>
                <DateInput 
                    label='Auction end date/time' 
                    name='auctionEnd' 
                    dateFormat={'dd MMMM yyyy h:mm a'}
                    showTimeSelect 
                    control={control} 
                    rules={{required:'Auction end date is required'}}/>

            </div>
        </>
        )}
        <div className='flex justify-between'>
            <Button outline color='gray'>Cancel</Button>
            <Button isProcessing={isSubmitting} 
            outline 
            disabled ={!isValid}
            type='submit'
            color='success'>Submit</Button>

        </div>
    </form>
  )
}
