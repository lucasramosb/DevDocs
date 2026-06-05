import { NextRequest, NextResponse } from "next/server";

const apiUrl =
  process.env.DEVDOCS_API_URL ??
  process.env.NEXT_PUBLIC_API_URL ??
  "http://localhost:5150";

type RouteContext = {
  params: Promise<{
    path?: string[];
  }>;
};

async function proxy(request: NextRequest, context: RouteContext) {
  const { path = [] } = await context.params;
  const targetUrl = new URL(path.join("/"), apiUrl.endsWith("/") ? apiUrl : `${apiUrl}/`);
  targetUrl.search = request.nextUrl.search;

  const hasBody = request.method !== "GET" && request.method !== "HEAD";

  try {
    const response = await fetch(targetUrl, {
      method: request.method,
      headers: {
        Accept: "application/json",
        "Content-Type": request.headers.get("Content-Type") ?? "application/json"
      },
      body: hasBody ? await request.text() : undefined,
      cache: "no-store"
    });

    const body = await response.text();

    return new NextResponse(body, {
      status: response.status,
      headers: {
        "Content-Type": response.headers.get("Content-Type") ?? "application/json"
      }
    });
  } catch {
    return NextResponse.json(
      { message: "Backend indisponível. Usando fallback mockado no cliente." },
      { status: 503 }
    );
  }
}

export const GET = proxy;
export const POST = proxy;
export const PUT = proxy;
export const PATCH = proxy;
export const DELETE = proxy;
