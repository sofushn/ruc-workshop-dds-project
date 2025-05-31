import os
import pandas as pd
import matplotlib.pyplot as plt
from plots import find_results
import numpy as np

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
            csvData = pd.read_csv(results_csv, low_memory=False)
            metric_name = 'http_req_duration'
            duration_data = csvData[csvData['metric_name'] == metric_name]
            if duration_data.empty:
                continue

            # Get all durations, sort, and compute percentiles
            durations = duration_data['metric_value'].sort_values().to_numpy()
            percentiles = np.linspace(0, 100, len(durations), endpoint=False)
            plt.plot(
                percentiles,
                durations,
                label=scenario.replace('_', ' ').title()
            )
            found_any = True

        if found_any:
            plt.xlabel('Percentile (%)')
            plt.ylabel('Duration (ms)')
            plt.yscale('log')
            plt.title(f'HTTP Request Duration Percentiles\nTest: {testtype} | Endpoint: {endpoint}')
            plt.legend()
            plt.tight_layout()
            out_path = os.path.join(output_dir, f"{testtype}_{endpoint}_duration_percentiles.svg")
            plt.savefig(out_path)
        plt.close()