import os
import pandas as pd
import matplotlib.pyplot as plt
from plots import find_results

scenarios = [
    'single_large_results',
    'single_small_results',
    'multi_results',
]
testtypes = ['breakpoint', 'spike', 'stress']
endpoints = ['Combined', 'GetMapById', 'GetWaypointsByMapId', 'PostWaypoint', 'GetImageById']

# Output directory for plots
output_dir = os.path.join('..', '..', '..', 'docs', 'plot-images')
os.makedirs(output_dir, exist_ok=True)

# Build the results dictionary
all_results = {}
for scenario in scenarios:
    base_path = os.path.join(scenario, 'results')
    if os.path.isdir(base_path):
        all_results.update(find_results(base_path))

for testtype in testtypes:
    for endpoint in endpoints:
        plt.figure(figsize=(12, 6))
        found_any = False

        for scenario in scenarios:
            key = f"{scenario}/{testtype}/{endpoint}"
            if key not in all_results:
                continue
            results_csv = all_results[key]
            test_info_txt = os.path.join(os.path.dirname(results_csv), 'test_info.txt')
            test_type_name = None
            with open(test_info_txt, 'r') as f:
                for line in f:
                    if line.startswith('TEST_TYPE:'):
                        test_type_name = line.split(':', 1)[1].strip()
                        break
            csvData = pd.read_csv(results_csv, low_memory=False)
            metric_name = 'http_req_duration'
            duration_data = csvData[csvData['metric_name'] == metric_name]
            if duration_data.empty:
                continue
            min_timestamp = duration_data['timestamp'].min()
            duration_data = duration_data.copy()
            duration_data['timestamp'] = duration_data['timestamp'] - min_timestamp
            duration_grouped = duration_data.groupby('timestamp')['metric_value'].mean().reset_index()
            plt.plot(
                duration_grouped['timestamp'],
                duration_grouped['metric_value'],
                label=scenario.replace('_', ' ').title()
            )
            found_any = True

        if found_any:
            plt.xlabel('Timestamp (Seconds)')
            plt.ylabel('Avg Duration (ms)')
            plt.yscale('log')
            plt.title(f'Average HTTP Request Duration Over Time\nTest: {testtype} | Endpoint: {endpoint}')
            plt.legend()
            plt.tight_layout()
            out_path = os.path.join(output_dir, f"{testtype}_{endpoint}_duration_lines.svg")
            plt.savefig(out_path)
        plt.close()