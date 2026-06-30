"use client";

import useSWR from "swr";
import Link from "next/link";
import { motion } from "framer-motion";
import { FolderGit2, Calendar, GitBranch, Search, Plus, Terminal } from "lucide-react";

const fetcher = (url: string) => fetch(url).then((res) => res.json());

export default function ProjectsList() {
  const { data: projects, error } = useSWR("http://localhost:5150/projects", fetcher);

  if (error) return <div className="p-8 text-destructive">Falha ao carregar projetos.</div>;
  if (!projects) return (
    <div className="p-12 flex justify-center">
      <div className="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>
  );

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="flex items-center justify-between mb-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-white mb-2">Seus Projetos</h1>
          <p className="text-white/50">Gerencie e visualize a documentação gerada por IA dos seus repositórios.</p>
        </div>
        <Link href="/" className="px-4 py-2 bg-primary/10 text-primary border border-primary/20 rounded-xl hover:bg-primary/20 transition-colors flex items-center gap-2">
          <Plus className="w-4 h-4" /> Novo Projeto
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {projects.map((project: any, idx: number) => (
          <motion.div
            key={project.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: idx * 0.1 }}
          >
            <Link href={`/projects/${project.id}`}>
              <div className="group h-full bg-card/40 backdrop-blur-sm border border-white/10 rounded-2xl p-6 hover:bg-white/[0.04] hover:border-primary/50 transition-all cursor-pointer relative overflow-hidden">
                
                {/* Hover Glow */}
                <div className="absolute inset-0 bg-gradient-to-br from-primary/10 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />

                <div className="relative z-10">
                  <div className="flex items-start justify-between mb-4">
                    <div className="w-10 h-10 rounded-xl bg-white/5 border border-white/10 flex items-center justify-center text-white/70 group-hover:text-primary group-hover:border-primary/30 transition-colors">
                      <FolderGit2 className="w-5 h-5" />
                    </div>
                  </div>
                  
                  <h3 className="text-xl font-semibold text-white/90 mb-1 group-hover:text-white transition-colors line-clamp-1">
                    {project.name}
                  </h3>
                  <p className="text-sm text-white/40 mb-6 font-mono flex items-center gap-1.5">
                    {project.owner} / {project.repositoryName}
                  </p>

                  <div className="flex items-center gap-4 text-xs text-white/30 font-medium">
                    <span className="flex items-center gap-1.5 bg-white/5 px-2 py-1 rounded-md">
                      <GitBranch className="w-3.5 h-3.5" />
                      {project.defaultBranch}
                    </span>
                    <span className="flex items-center gap-1.5">
                      <Calendar className="w-3.5 h-3.5" />
                      {new Date(project.createdAt).toLocaleDateString("pt-BR")}
                    </span>
                  </div>
                </div>
              </div>
            </Link>
          </motion.div>
        ))}

        {projects.length === 0 && (
          <div className="col-span-full py-20 flex flex-col items-center justify-center text-white/30 border border-dashed border-white/10 rounded-3xl">
            <Terminal className="w-12 h-12 mb-4 opacity-50" />
            <p>Nenhum projeto analisado ainda.</p>
          </div>
        )}
      </div>
    </div>
  );
}
