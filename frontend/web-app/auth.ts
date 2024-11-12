import NextAuth, { Profile } from "next-auth"
import { OIDCConfig } from "next-auth/providers"
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"
export const { handlers, signIn, signOut, auth } = NextAuth({
    session: {
        strategy: 'jwt'
    },
  providers: [
    DuendeIDS6Provider({
        id: 'id-server',
        clientId: "nextApp",
        clientSecret: "secretkey",
        issuer: "http://localhost:5000",
        authorization: {params: {scope:'openid profile auctionApp'}},
        idToken: true
        //omit: exclude something
      } as OIDCConfig<Omit<Profile,'username'>>),
  ],
  callbacks: {
    async authorized({auth}){
      return !!auth
    },
    async jwt({token, profile, account}){
      //console.log({token, user, account, profile });
      if (account && account.access_token){
        token.accessToken = account.access_token
      }
      if (profile){
        //populate username into token
        token.username = profile.username
      }
      return token;
    },
    async session({session, token}){
      if(token){
        session.user.username = token.username;
        session.accessToken = token.accessToken;
      }
      return session;
    }
  }
})
