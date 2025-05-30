import os
import pandas as pd
import matplotlib.pyplot as plt
from plots import find_results

# Define the scenarios and testtypes/endpoints you want to compare
scenarios = [
    'single_large_results',
    'single_small_results',
    'multi_results',
]
testtypes = ['breakpoint', 'spike', 'stress']
endpoints = ['Combined', 'GetMapById', 'GetWaypointsByMapId', 'PostWaypoint', 'GetImageById']

# Build the results dictionary
all_results = {}
for scenario in scenarios:
    base_path = os.path.join(scenario, 'results')
    if os.path.isdir(base_path):
        all_results.update(find_results(base_path))

# For each testtype/endpoint, create a multiplot
for testtype in testtypes:
    for endpoint in endpoints:
        fig, axes = plt.subplots(3, 1, figsize=(12, 10), sharex=True)
        found_any = False

        # First pass: collect all counts to determine vmin/vmax
        all_counts = []
        check_grouped_list = [None, None, None]
        min_timestamp_list = [None, None, None]
        test_type_name_list = [None, None, None]
        for i, scenario in enumerate(scenarios):
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
            test_type_name_list[i] = test_type_name
            csvData = pd.read_csv(results_csv, low_memory=False)
            metric_name = 'http_req_duration'
            duration_data = csvData[csvData['metric_name'] == metric_name]
            if duration_data.empty:
                continue
            min_timestamp = duration_data['timestamp'].min()
            min_timestamp_list[i] = min_timestamp
            if 'check' in csvData.columns:
                check_status = 'status is 201' if test_type_name == 'PostWaypoint' else 'status is 200'
                check_data = csvData[csvData['check'] == check_status]
                if not check_data.empty:
                    check_data = check_data.copy()
                    check_data['timestamp'] = check_data['timestamp'] - min_timestamp
                    check_grouped = check_data.groupby('timestamp')['metric_value'].agg(['mean', 'count']).reset_index()
                    check_grouped_list[i] = check_grouped
                    all_counts.extend(check_grouped['count'].tolist())

        # Determine global vmin/vmax for color normalization
        vmin = min(all_counts) if all_counts else 0
        vmax = max(all_counts) if all_counts else 1

        # Second pass: plot
        for i, scenario in enumerate(scenarios):
            key = f"{scenario}/{testtype}/{endpoint}"
            if key not in all_results:
                axes[i].set_title(f"{scenario}: No data")
                axes[i].axis('off')
                continue

            check_grouped = check_grouped_list[i]
            test_type_name = test_type_name_list[i]
            if check_grouped is not None:
                found_any = True
                scatter = axes[i].scatter(
                    check_grouped['timestamp'],
                    (1 - check_grouped['mean']) * 100,
                    c=check_grouped['count'],
                    cmap='viridis',
                    vmin=vmin,
                    vmax=vmax,
                    label='Fail Rate (%) (color: # requests)'
                )
                cbar = plt.colorbar(scatter, ax=axes[i])
                cbar.set_label('Number of Requests')
                axes[i].set_ylabel('Fail Rate (%)')
                axes[i].set_ylim(0, 100)
                axes[i].set_title(f"{scenario}")
                axes[i].legend()
            else:
                axes[i].set_title(f"{scenario}: No check data")
                axes[i].axis('off')

        if found_any:
            axes[-1].set_xlabel('Timestamp (Seconds)')
            plt.suptitle(f'Check Fail Rate Over Time\nTest: {testtype} | Endpoint: {endpoint}', fontsize=16)
            plt.tight_layout(rect=[0, 0.03, 1, 0.95])
            plt.savefig(f"{testtype}_{endpoint}_check_multiplot.png")
        plt.close()