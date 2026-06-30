"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { FolderGit2, Sparkles, Code2, Layers, Cpu, ArrowRight } from "lucide-react";

export default function Home() {
  const [url, setUrl] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const router = useRouter();

  const handleAnalyze = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!url) return;

    setIsLoading(true);
    setError("");

    try {
      const res = await fetch("http://localhost:5150/projects/analyze", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ githubUrl: url }),
      });

      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(errorText || "Erro ao iniciar análise.");
      }

      const data = await res.json();
      // Returns { projectId, analysisJobId, status }
      router.push(`/projects/${data.projectId}/analyze/${data.analysisJobId}`);
    } catch (err: any) {
      setError(err.message || "Erro de conexão com o servidor.");
      setIsLoading(false);
    }
  };

  return (
    <div className="flex-1 flex flex-col items-center justify-center relative overflow-hidden px-4">
      
      {/* Glow Effects */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-primary/20 rounded-full blur-[120px] pointer-events-none" />

      <motion.div 
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.7, ease: "easeOut" }}
        className="max-w-3xl w-full flex flex-col items-center text-center z-10"
      >
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/5 border border-white/10 text-white/70 text-sm mb-8 backdrop-blur-sm">
          <Sparkles className="w-4 h-4 text-primary" />
          <span>Documentação alimentada por IA Local (Ollama)</span>
        </div>

        <h1 className="text-5xl md:text-7xl font-bold tracking-tighter mb-6 bg-gradient-to-br from-white via-white/90 to-white/30 bg-clip-text text-transparent">
          Compreenda qualquer <br className="hidden md:block" />
          <span className="text-primary drop-shadow-[0_0_20px_rgba(91,141,255,0.4)]">repositório GitHub</span>
        </h1>
        
        <p className="text-lg text-white/50 mb-12 max-w-2xl font-light">
          Cole a URL de um repositório público e deixe a Inteligência Artificial mapear a arquitetura, explicar os fluxos e documentar cada arquivo do sistema.
        </p>

        <form onSubmit={handleAnalyze} className="w-full max-w-xl relative group">
          <div className="absolute -inset-0.5 bg-gradient-to-r from-primary/50 to-cyan-400/50 rounded-2xl blur opacity-30 group-hover:opacity-60 transition duration-500"></div>
          <div className="relative flex items-center bg-card border border-white/10 rounded-2xl overflow-hidden shadow-2xl">
            <div className="pl-4 text-white/40">
              <FolderGit2 className="w-6 h-6" />
            </div>
            <input 
              type="url" 
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="https://github.com/owner/repo" 
              required
              disabled={isLoading}
              className="w-full bg-transparent border-none text-white px-4 py-4 focus:outline-none placeholder:text-white/20 font-mono text-sm"
            />
            <button 
              type="submit"
              disabled={isLoading}
              className="h-full px-6 bg-primary text-primary-foreground font-medium hover:bg-primary/90 transition-colors flex items-center gap-2 whitespace-nowrap disabled:opacity-50"
            >
              {isLoading ? (
                <span className="animate-pulse">Analisando...</span>
              ) : (
                <>
                  Iniciar <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </div>
          {error && (
            <div className="absolute top-full left-0 mt-3 text-destructive text-sm font-medium">
              {error}
            </div>
          )}
        </form>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-24 w-full text-left">
          <FeatureCard 
            icon={<Layers className="w-6 h-6 text-primary" />}
            title="Mapeamento Estrutural"
            desc="Varredura completa da árvore de arquivos e identificação automática de tecnologias."
          />
          <FeatureCard 
            icon={<Code2 className="w-6 h-6 text-primary" />}
            title="Análise de Código"
            desc="Modelos de IA locais analisam o código-fonte para extrair a lógica de negócio."
          />
          <FeatureCard 
            icon={<Cpu className="w-6 h-6 text-primary" />}
            title="Geração de Docs"
            desc="Documentação rica gerada automaticamente em Markdown para onboarding."
          />
        </div>
      </motion.div>
    </div>
  );
}

function FeatureCard({ icon, title, desc }: { icon: React.ReactNode, title: string, desc: string }) {
  return (
    <div className="p-6 rounded-2xl bg-white/[0.02] border border-white/5 hover:bg-white/[0.04] transition-colors">
      <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center mb-4">
        {icon}
      </div>
      <h3 className="text-white/90 font-semibold mb-2">{title}</h3>
      <p className="text-white/40 text-sm leading-relaxed">{desc}</p>
    </div>
  );
}
