import os
import json
import pandas as pd
import matplotlib.pyplot as plt

def find_summaries(base_path):
    results = {}
    for root, dirs, files in os.walk(base_path):
        if 'summary.json' in files and 'test_info.txt' in files:
            # Read TEST_TYPE from test_info.txt
            test_info_path = os.path.join(root, 'test_info.txt')
            test_type = None
            with open(test_info_path, 'r') as f:
                for line in f:
                    if line.startswith('TEST_TYPE:'):
                        test_type = line.split(':', 1)[1].strip()
                        break
            if not test_type:
                test_type = "unknown"

            # Build key: <topdir>/<testtype>/<TEST_TYPE>
            parts = root.split(os.sep)
            try:
                idx = parts.index('results')
                topdir = parts[idx-1]
                testtype = parts[idx+1]
                key = f"{topdir}/{testtype}/{test_type}"
            except Exception:
                key = f"{root}/{test_type}"

            results[key] = os.path.join(root, 'summary.json')
    return results

# Define scenarios and testtypes/endpoints
scenarios = [
    'single_large_results',
    'single_small_results',
    'multi_results',
]
testtypes = ['breakpoint', 'spike', 'stress']
endpoints = ['Combined', 'GetMapById', 'GetWaypointsByMapId', 'PostWaypoint', 'GetImageById']

# Output directory for tables
output_dir = os.path.join('..', '..', '..', 'docs', 'table-images')
os.makedirs(output_dir, exist_ok=True)

# Build the summary dictionary
all_summaries = {}
for scenario in scenarios:
    base_path = os.path.join(scenario, 'results')
    if os.path.isdir(base_path):
        all_summaries.update(find_summaries(base_path))

# For each testtype/endpoint, create a table
for testtype in testtypes:
    for endpoint in endpoints:
        rows = []
        index_labels = []
        for scenario in scenarios:
            key = f"{scenario}/{testtype}/{endpoint}"
            if key not in all_summaries:
                continue
            summary_path = all_summaries[key]
            with open(summary_path, 'r') as f:
                summary = json.load(f)
            metrics = summary.get('metrics', {})
            # Try endpoint-specific, fallback to global
            dur_key = f"http_req_duration{{endpoint:{endpoint}}}"
            dur = metrics.get(dur_key, metrics.get("http_req_duration", {}))
            http_req_failed = metrics.get("http_req_failed", {})
            http_reqs = metrics.get("http_reqs", {})
            # Duration values
            med = dur.get("med", None)
            p90 = dur.get("p(90)", None)
            p95 = dur.get("p(95)", None)
            # http_req_failed percent (as success percent)
            fail_value = http_req_failed.get("value", None)
            pass_percent = (100 - fail_value * 100) if fail_value is not None else None
            # Request count
            req_count = http_reqs.get("count", None)

            # Format duration values to 2 decimal points, percent as 2 decimals
            def fmt_float(val):
                if val is None:
                    return ""
                return f"{val:.2f}"
            def fmt_int(val):
                if val is None:
                    return ""
                return f"{int(val)}"

            rows.append([
                fmt_float(med), fmt_float(p90), fmt_float(p95),
                fmt_float(pass_percent), fmt_int(req_count)
            ])
            index_labels.append(scenario.replace('_results', '').replace('_', ' ').title())

        if not rows:
            continue

        df = pd.DataFrame(
            rows,
            columns=[
                "Median (ms)", "p90 (ms)", "p95 (ms)",
                "Request Success %", "Request Count"
            ],
            index=index_labels
        )

        # Plot as table and save as SVG
        fig, ax = plt.subplots(figsize=(10, 0.7 + len(df) * 0.4))
        ax.axis('off')
        tbl = ax.table(
            cellText=df.values,
            colLabels=df.columns,
            rowLabels=df.index,
            loc='center',
            cellLoc='center'
        )
        tbl.auto_set_font_size(False)
        tbl.set_fontsize(10)
        tbl.scale(1.2, 1.2)
        # plt.tight_layout()  # Optionally remove this
        plt.subplots_adjust(top=1, bottom=0, left=0, right=1)
        out_path = os.path.join(output_dir, f"{testtype}_{endpoint}_summary_table.svg")
        plt.savefig(out_path, bbox_inches='tight')
        plt.close()