'use server'
import { fetchWrapper } from "@/lib/fetchWrapper";
import { PagedResult, Auction } from "@/types";
import { revalidatePath } from "next/cache";
import {  FieldValues } from "react-hook-form";

export async function getData(query: string): Promise<PagedResult<Auction>> {
    // const res = await fetch(`http://localhost:6001/search${query}`);
    // if (!res.ok) throw new Error('Failed to fetch data');
    // return res.json();
    return await fetchWrapper.get(`search${query}`)
}

//for testing only
export async function updateAuctionTest(){
    const data = {
        mileage: Math.floor(Math.random()* 10000) +1
    }
    return await fetchWrapper.put('auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c', data);
}

export async function createAuction(data: FieldValues){
    return fetchWrapper.post('auctions', data)
}
// return from any to auction object
export async function getDetailedViewData(id: string): Promise<Auction>{
    return await fetchWrapper.get(`auctions/${id}`)
}

export async function updateAuction(data: FieldValues, id: string): Promise<Auction>{
    const res = await fetchWrapper.put(`auctions/${id}`, data);
    revalidatePath(`/auction/${id}`);
    return res;
}

export async function deleteAuction( id: string){
    const res = await fetchWrapper.del(`auctions/${id}`);
    return res;
}

