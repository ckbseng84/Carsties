import { getCurrentUser } from "./actions/authAction";
import "./globals.css";
import NavBar from "./nav/NavBar";
import SignalRProvider from "./providers/SignalRProvider";
import ToasterProvider from "./providers/ToasterProvider";

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const user = await getCurrentUser()
  return (
    <html lang="en">
      <body>
        <ToasterProvider/>
        <NavBar></NavBar>
        <main className="container mx-auto px-5 pt-10">
          <SignalRProvider user={user}>
            {children}
          </SignalRProvider>
          </main>
      </body>
    </html>
  );
}
