import type { Metadata } from "next";
import { IBM_Plex_Mono, Space_Grotesk } from "next/font/google";

import "@/app/globals.css";

const spaceGrotesk = Space_Grotesk({
  subsets: ["latin"],
  variable: "--font-sans"
});

const ibmPlexMono = IBM_Plex_Mono({
  subsets: ["latin"],
  weight: ["400", "500", "600"],
  variable: "--font-mono"
});

export const metadata: Metadata = {
  title: "DevDocs",
  description: "Documentação técnica automatizada para repositórios GitHub."
};

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" className="dark">
      <body className={`${spaceGrotesk.variable} ${ibmPlexMono.variable} antialiased selection:bg-primary/30 min-h-screen flex flex-col`}>
        <header className="fixed top-0 w-full z-50 border-b border-white/5 bg-background/60 backdrop-blur-md">
          <div className="container mx-auto px-4 h-16 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-lg bg-primary/20 flex items-center justify-center border border-primary/50 shadow-[0_0_15px_rgba(91,141,255,0.4)]">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-primary"><path d="m18 16 4-4-4-4"/><path d="m6 8-4 4 4 4"/><path d="m14.5 4-5 16"/></svg>
              </div>
              <a href="/" className="text-lg font-bold tracking-tight text-white/90 hover:text-white transition-colors flex items-center gap-2">
                DevDocs
              </a>
            </div>
            <nav className="flex items-center gap-6 text-sm font-medium">
              <a href="/projects" className="text-white/60 hover:text-white hover:drop-shadow-[0_0_8px_rgba(255,255,255,0.5)] transition-all">Projetos Analisados</a>
              <a href="https://github.com" target="_blank" rel="noreferrer" className="text-white/60 hover:text-white transition-all">GitHub</a>
            </nav>
          </div>
        </header>
        <main className="flex-1 pt-16 flex flex-col">
          {children}
        </main>
      </body>
    </html>
  );
}
