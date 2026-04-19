import type { Metadata } from "next";
import { Cormorant_Garamond, Outfit, Geist_Mono } from "next/font/google";
import { Toaster } from "react-hot-toast";
import { AuthProvider } from "@/context/AuthContext";
import NavBar from "@/components/layout/NavBar";
import ProfileGuard from "@/components/auth/ProfileGuard";
import "./globals.css";

const cormorant = Cormorant_Garamond({
  variable: "--font-cormorant",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const outfit = Outfit({
  variable: "--font-outfit",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Digital Menu - Personalised Nutrition",
  description: "Digital Restaurant Menus with Personalised Nutrition Feedback",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const globalBgImage = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=1200&q=80";

  return (
    <html lang="en">
      <body
        className={`${cormorant.variable} ${outfit.variable} ${geistMono.variable} antialiased selection:bg-accent/30`}
      >
        <AuthProvider>
          {/* Global Immersive Background Layer */}
          <div className="fixed inset-0 -z-50 w-full h-full overflow-hidden pointer-events-none">
            <div
              className="absolute inset-0 bg-cover bg-center brightness-[0.9]"
              style={{ backgroundImage: `url(${globalBgImage})` }}
            />
            {/* Luxury Overlay: Slightly lighter than restaurant page to keep the "Light" luxury feel */}
            <div className="absolute inset-0 bg-bg-primary/85 backdrop-blur-[2px]" />
          </div>

          <NavBar />
          <main className="min-h-screen pb-20 md:pb-0 relative z-10">
            <ProfileGuard>
              {children}
            </ProfileGuard>
          </main>

          <Toaster
            position="top-right"
            toastOptions={{
              style: {
                background: "#FFFFFF",
                color: "#1C1917",
                border: "1px solid #C1B8A8",
              },
            }}
          />
        </AuthProvider>
      </body>
    </html>
  );
}
