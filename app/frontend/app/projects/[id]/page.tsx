"use client";

import useSWR from "swr";
import { use } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { ArrowLeft, BookOpen, Layers, GitMerge, FileCode2, Zap, Clock } from "lucide-react";
import Link from "next/link";
import { motion } from "framer-motion";

const fetcher = (url: string) => fetch(url).then((res) => {
  if (!res.ok) throw new Error("Documentação não encontrada.");
  return res.json();
});

export default function ProjectDocumentation({ params }: { params: Promise<{ id: string }> }) {
  const unwrappedParams = use(params);
  const { data: doc, error } = useSWR(`http://localhost:5150/projects/${unwrappedParams.id}/documentation`, fetcher);

  if (error) {
    return (
      <div className="container mx-auto px-4 py-12">
        <Link href="/projects" className="inline-flex items-center gap-2 text-white/50 hover:text-white mb-8 transition-colors">
          <ArrowLeft className="w-4 h-4" /> Voltar
        </Link>
        <div className="bg-destructive/10 border border-destructive/20 text-destructive p-6 rounded-2xl">
          <h2 className="text-xl font-bold mb-2">Ops!</h2>
          <p>{error.message}</p>
        </div>
      </div>
    );
  }

  if (!doc) {
    return (
      <div className="container mx-auto px-4 py-20 flex justify-center">
        <div className="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-12 max-w-6xl">
      <Link href="/projects" className="inline-flex items-center gap-2 text-white/50 hover:text-white mb-8 transition-colors">
        <ArrowLeft className="w-4 h-4" /> Voltar para Projetos
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        
        {/* Sidebar Info */}
        <div className="lg:col-span-1 space-y-6">
          <div className="bg-card/40 backdrop-blur-sm border border-white/10 p-6 rounded-3xl sticky top-24">
            <div className="w-12 h-12 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary mb-6">
              <BookOpen className="w-6 h-6" />
            </div>
            
            <h1 className="text-2xl font-bold text-white mb-2">{doc.title}</h1>
            <p className="text-white/50 text-sm mb-6">{doc.overview}</p>

            <div className="space-y-4">
              <div className="flex items-start gap-3">
                <Zap className="w-4 h-4 text-white/30 mt-0.5 shrink-0" />
                <div>
                  <div className="text-xs font-semibold text-white/30 uppercase tracking-wider mb-1">Tecnologias</div>
                  <div className="text-sm text-white/80">{doc.technologies}</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <Clock className="w-4 h-4 text-white/30 mt-0.5 shrink-0" />
                <div>
                  <div className="text-xs font-semibold text-white/30 uppercase tracking-wider mb-1">Gerado em</div>
                  <div className="text-sm text-white/80">{new Date(doc.createdAt).toLocaleDateString("pt-BR")}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Markdown Content */}
        <div className="lg:col-span-3">
          <motion.div 
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-card/20 backdrop-blur-sm border border-white/5 rounded-3xl p-8 md:p-12"
          >
            <div className="markdown-body">
              <ReactMarkdown 
                remarkPlugins={[remarkGfm]}
                components={{
                  h1: ({node, ...props}) => <h1 className="text-3xl font-bold text-white mb-6 pb-2 border-b border-white/10" {...props} />,
                  h2: ({node, ...props}) => <h2 className="text-2xl font-semibold text-white/90 mt-10 mb-4" {...props} />,
                  h3: ({node, ...props}) => <h3 className="text-xl font-medium text-white/80 mt-8 mb-3" {...props} />,
                  p: ({node, ...props}) => <p className="text-white/60 leading-relaxed mb-4" {...props} />,
                  ul: ({node, ...props}) => <ul className="list-disc list-inside text-white/60 mb-4 space-y-1" {...props} />,
                  ol: ({node, ...props}) => <ol className="list-decimal list-inside text-white/60 mb-4 space-y-1" {...props} />,
                  li: ({node, ...props}) => <li className="text-white/60" {...props} />,
                  a: ({node, ...props}) => <a className="text-primary hover:underline" {...props} />,
                  code: ({node, className, children, ...props}) => {
                    const match = /language-(\w+)/.exec(className || "");
                    const isInline = !match && !className;
                    return isInline ? (
                      <code className="bg-white/10 text-white/80 px-1.5 py-0.5 rounded-md text-sm font-mono" {...props}>{children}</code>
                    ) : (
                      <div className="my-4 rounded-xl overflow-hidden border border-white/10">
                        <div className="bg-white/5 px-4 py-2 border-b border-white/10 text-xs text-white/40 font-mono flex items-center justify-between">
                          <span>{match?.[1] || "code"}</span>
                          <FileCode2 className="w-3.5 h-3.5" />
                        </div>
                        <pre className="bg-black/40 p-4 overflow-x-auto">
                          <code className={`text-sm text-white/80 font-mono ${className}`} {...props}>
                            {children}
                          </code>
                        </pre>
                      </div>
                    )
                  },
                  table: ({node, ...props}) => (
                    <div className="overflow-x-auto mb-6">
                      <table className="w-full text-left border-collapse" {...props} />
                    </div>
                  ),
                  th: ({node, ...props}) => <th className="border-b border-white/10 py-3 px-4 text-sm font-semibold text-white/80" {...props} />,
                  td: ({node, ...props}) => <td className="border-b border-white/5 py-3 px-4 text-sm text-white/60" {...props} />,
                  blockquote: ({node, ...props}) => <blockquote className="border-l-4 border-primary/50 pl-4 italic text-white/50 mb-4 bg-primary/5 py-2 pr-4 rounded-r-lg" {...props} />,
                }}
              >
                {doc.content}
              </ReactMarkdown>
            </div>
          </motion.div>
        </div>

      </div>
    </div>
  );
}
