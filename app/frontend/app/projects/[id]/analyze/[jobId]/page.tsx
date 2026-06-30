"use client";

import { useEffect, useState, use } from "react";
import { useRouter } from "next/navigation";
import useSWR from "swr";
import { motion, AnimatePresence } from "framer-motion";
import { Loader2, CheckCircle2, AlertCircle, FileText, DownloadCloud, BrainCircuit, CheckCircle } from "lucide-react";

const fetcher = (url: string) => fetch(url).then((res) => res.json());

export default function AnalysisProgress({ params }: { params: Promise<{ id: string; jobId: string }> }) {
  const router = useRouter();
  const unwrappedParams = use(params);
  
  // Polling a cada 2 segundos
  const { data: job, error } = useSWR(
    `http://localhost:5150/projects/${unwrappedParams.id}/analysis-jobs/${unwrappedParams.jobId}`,
    fetcher,
    { refreshInterval: 2000 }
  );

  useEffect(() => {
    if (job?.status === "Completed") {
      // Redireciona para a página do projeto
      router.push(`/projects/${unwrappedParams.id}`);
    }
  }, [job, router, unwrappedParams.id]);

  if (error) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <div className="text-destructive flex items-center gap-2 bg-destructive/10 px-4 py-3 rounded-xl border border-destructive/20">
          <AlertCircle className="w-5 h-5" />
          <span>Erro ao buscar status do job.</span>
        </div>
      </div>
    );
  }

  if (!job) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="w-8 h-8 text-primary animate-spin" />
      </div>
    );
  }

  const steps = [
    { id: "Pending", label: "Aguardando Worker", icon: <Loader2 className="w-5 h-5" /> },
    { id: "DownloadingRepository", label: "Mapeando Arquivos", icon: <DownloadCloud className="w-5 h-5" /> },
    { id: "MappingFiles", label: "Salvando Arquivos", icon: <FileText className="w-5 h-5" /> },
    { id: "GeneratingFileDocumentation", label: "Gerando Docs (Ollama)", icon: <BrainCircuit className="w-5 h-5" /> },
    { id: "GeneratingProjectDocumentation", label: "Finalizando Visão Geral", icon: <CheckCircle className="w-5 h-5" /> },
  ];

  // Helper para saber o status do step atual
  const getStepStatus = (stepId: string) => {
    if (job.status === "Failed") return stepId === job.currentStep ? "error" : "idle";
    const stepIndex = steps.findIndex(s => s.id === stepId);
    const currentIndex = steps.findIndex(s => s.id === job.currentStep);
    
    if (job.status === "Completed") return "completed";
    if (stepIndex < currentIndex) return "completed";
    if (stepIndex === currentIndex) return "active";
    return "idle";
  };

  return (
    <div className="flex-1 flex flex-col items-center justify-center px-4 relative">
      <div className="w-[800px] max-w-full">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          className="bg-card/50 backdrop-blur-xl border border-white/10 p-10 rounded-3xl shadow-2xl relative overflow-hidden"
        >
          {/* Subtle glow inside card */}
          <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[300px] h-[300px] bg-primary/10 rounded-full blur-[80px] pointer-events-none" />

          <div className="text-center mb-12 relative z-10">
            <h2 className="text-3xl font-bold text-white mb-3">Analisando Código-Fonte</h2>
            <p className="text-white/50">A Inteligência Artificial está processando seu projeto em background.</p>
          </div>

          <div className="space-y-6 relative z-10">
            {steps.map((step, idx) => {
              const status = getStepStatus(step.id);
              
              return (
                <div key={step.id} className="relative flex items-center gap-4">
                  {/* Line connector */}
                  {idx !== steps.length - 1 && (
                    <div className={`absolute left-[1.375rem] top-10 bottom-[-1.5rem] w-px ${status === "completed" ? "bg-primary" : "bg-white/10"}`} />
                  )}
                  
                  <div className={`
                    w-11 h-11 rounded-full flex items-center justify-center shrink-0 border-2 transition-all duration-500
                    ${status === "completed" ? "bg-primary/20 border-primary text-primary shadow-[0_0_15px_rgba(91,141,255,0.4)]" : 
                      status === "active" ? "bg-white/10 border-white/30 text-white animate-pulse" : 
                      status === "error" ? "bg-destructive/20 border-destructive text-destructive" :
                      "bg-transparent border-white/10 text-white/30"}
                  `}>
                    {status === "completed" ? <CheckCircle2 className="w-5 h-5" /> : step.icon}
                  </div>
                  
                  <div className="flex-1">
                    <h4 className={`font-medium text-lg ${status === "active" ? "text-white" : status === "completed" ? "text-white/90" : "text-white/40"}`}>
                      {step.label}
                    </h4>
                    
                    <AnimatePresence>
                      {status === "active" && step.id === "GeneratingFileDocumentation" && (
                        <motion.div 
                          initial={{ height: 0, opacity: 0 }}
                          animate={{ height: "auto", opacity: 1 }}
                          exit={{ height: 0, opacity: 0 }}
                          className="mt-3"
                        >
                          <div className="flex justify-between text-sm text-white/60 mb-2 font-mono">
                            <span>Processados: {job.filesProcessed} / {job.filesFound}</span>
                            <span>{job.progress}%</span>
                          </div>
                          <div className="h-2 w-full bg-white/5 rounded-full overflow-hidden">
                            <motion.div 
                              className="h-full bg-gradient-to-r from-primary to-cyan-400"
                              initial={{ width: 0 }}
                              animate={{ width: `${(job.filesProcessed / (job.filesFound || 1)) * 100}%` }}
                              transition={{ duration: 0.5 }}
                            />
                          </div>
                        </motion.div>
                      )}
                    </AnimatePresence>

                    {status === "error" && (
                      <div className="mt-2 text-destructive text-sm bg-destructive/10 p-3 rounded-lg border border-destructive/20">
                        {job.errorMessage || "Ocorreu um erro desconhecido durante o processamento."}
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </motion.div>
      </div>
    </div>
  );
}
