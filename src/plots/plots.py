import os

def find_results(base_path):
    results = {}
    for root, dirs, files in os.walk(base_path):
        if 'results.csv' in files and 'test_info.txt' in files:
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
            # Example: multi_results/breakpoint/GetMapById
            parts = root.split(os.sep)
            try:
                idx = parts.index('results')
                topdir = parts[idx-1]
                testtype = parts[idx+1]
                key = f"{topdir}/{testtype}/{test_type}"
            except Exception:
                key = f"{root}/{test_type}"

            results[key] = os.path.join(root, 'results.csv')
    return results

if __name__ == "__main__":
    base_dirs = [
        'single_large_results',
        'single_small_results',
        'multi_results',
    ]
    all_results = {}
    for base in base_dirs:
        base_path = os.path.join(base, 'results')
        if os.path.isdir(base_path):
            all_results.update(find_results(base_path))
    for k, v in all_results.items():
        print(f"{k}: {v}")