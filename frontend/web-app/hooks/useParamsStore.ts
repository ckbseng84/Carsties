import { create } from "zustand"

type State = {
    pageNumber: number
    pageSize: number
    PageCount: number
    searchTerm: string
    searchValue: string
    orderBy: string
    filterBy: string
}

type Actions = {
    setParam: (params: Partial<State>) => void
    reset:() => void
    setSearchValue: (value: string) => void
}

const initialState: State={
    pageNumber:1,
    pageSize: 12,
    PageCount:1,
    searchTerm: '',
    searchValue:'',
    orderBy: 'make',
    filterBy:'live'
}

export const useParamsStore = create<State & Actions>() ((set)=> ({
    ...initialState,
    setParam: (newParams: Partial<State>) => {
        set((state) => {
            if (newParams.pageNumber){
                return {...state, pageNumber: newParams.pageNumber};
            }else {
                return {...state, ...newParams, pageNumber: 1}
            }
        } )
    },
    reset: () => set(initialState),
    setSearchValue: (value:string) => {
        set({searchValue: value})
    }
}))