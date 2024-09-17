import "./globals.css";
import NavBar from "./nav/NavBar";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <NavBar></NavBar>
        <main className="container mx-auto px-5 pt-10">{children}</main>
      </body>
    </html>
  );
}
