import * as React from "react";

import { cn } from "@/lib/utils";

function Progress({
  value = 0,
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement> & { value?: number }) {
  return (
    <div
      className={cn(
        "relative h-2.5 w-full overflow-hidden rounded-full border border-white/10 bg-secondary/80 shadow-inner shadow-black/30",
        className
      )}
      {...props}
    >
      <div
        className="h-full rounded-full bg-[linear-gradient(90deg,hsl(var(--primary)),hsl(var(--accent)))] shadow-[0_0_18px_rgba(45,212,191,0.45)] transition-all duration-500"
        style={{ width: `${Math.max(0, Math.min(100, value))}%` }}
      />
    </div>
  );
}

export { Progress };
