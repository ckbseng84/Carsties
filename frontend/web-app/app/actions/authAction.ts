'use server'

import { auth } from "@/auth";

//run on server side only?
export async function getCurrentUser(){
    try {
        const session = await auth();
        if (!session) return null;
        return session.user;
    } catch (error) {
        return null;
        
    }
}