"use client";

import Link from "next/link";

interface BreadcrumbItem {
    label: string;
    href?: string; // No href = current page (not a link)
}

export default function Breadcrumbs({ items }: { items: BreadcrumbItem[] }) {
    return (
        <nav className="flex items-center gap-2 text-sm text-text-secondary mb-6 flex-wrap">
            {items.map((item, i) => (
                <span key={i} className="flex items-center gap-2">
                    {i > 0 && <span className="text-bg-elevated">/</span>}
                    {item.href ? (
                        <Link href={item.href} className="hover:text-accent transition-colors">
                            {item.label}
                        </Link>
                    ) : (
                        <span className="text-text-primary font-medium">{item.label}</span>
                    )}
                </span>
            ))}
        </nav>
    );
}
