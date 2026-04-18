import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { Toaster } from "react-hot-toast";
import { AuthProvider } from "@/context/AuthContext";
import NavBar from "@/components/layout/NavBar";
import ProfileGuard from "@/components/auth/ProfileGuard";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Digital Menu - Personalised Nutrittion",
  description: "Digital Restaurant Menus with Personalised Nutrition Feedback",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        <AuthProvider>
          <NavBar />
          <main className="min-h-screen pb-20 md:pb-0">
            <ProfileGuard>
              {children}
            </ProfileGuard>
          </main>
          <Toaster
            position="top-right"
            toastOptions={{
              style: {
                background: "#1A1D27",
                color: "#F9FAFB",
                border: "1px solid #252836",
              },
            }}
          />
        </AuthProvider>
      </body>
    </html>
  );
}
