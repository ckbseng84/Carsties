import { Bid } from "@/types"
import { create } from "zustand"

type State = {
    bids: Bid[],
    open: boolean,
}

type Actions = {
    setBids : (bids: Bid[]) => void
    addBid: (bid: Bid) => void
    setOpen: (value: boolean)=> void
    
}

export const useBidStore = create<State & Actions>((set) =>({
    bids: [],
    open: true,
    setBids: (bids: Bid[]) => {
        set(() => ({
            bids
        }))
    },
    addBid: (bid: Bid) => {
        //ensure the Bid is not duplicate in the array
        set((state) => ({
            bids : !state.bids.find(x => x.id ===bid.id) //search array by id
            ? [bid,...state.bids] // not found, insert new bid at first of array and add the rest of the array
            : [...state.bids] //else, just add the rest of the array
        }))
    },
    setOpen: (value: boolean)=> {
        set(()=> ({
            open: value
        }))
    }
}))