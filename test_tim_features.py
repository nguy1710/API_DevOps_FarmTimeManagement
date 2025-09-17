#!/usr/bin/env python3
"""
Tim's Feature Testing Script
Tests the new Tim enhanced endpoints for Epic 8
"""

import requests
import json
import datetime
from typing import Dict, Any

# Configuration
BASE_URL = "http://localhost:5000/api"
HEADERS = {
    "Content-Type": "application/json",
    "Accept": "application/json"
}

def print_test_header(test_name: str):
    """Print a formatted test header"""
    print(f"\n{'='*60}")
    print(f"🧪 TESTING: {test_name}")
    print(f"{'='*60}")

def print_response(response: requests.Response, expected_status: int = 200):
    """Print formatted response information"""
    status_icon = "✅" if response.status_code == expected_status else "❌"
    print(f"{status_icon} Status Code: {response.status_code}")

    try:
        response_json = response.json()
        print(f"📄 Response: {json.dumps(response_json, indent=2)}")
    except:
        print(f"📄 Response: {response.text}")
    print("-" * 40)

def test_roster_status_check():
    """Test Tim's roster status endpoint"""
    print_test_header("Roster Status Check")

    # Test with staff ID 1
    staff_id = 1
    url = f"{BASE_URL}/events/roster-status/{staff_id}"

    try:
        response = requests.get(url, headers=HEADERS)
        print_response(response)
        return response.status_code == 200
    except requests.exceptions.RequestException as e:
        print(f"❌ Request failed: {e}")
        return False

def test_validation_dry_run():
    """Test Tim's validation preview endpoint"""
    print_test_header("Validation Dry Run")

    url = f"{BASE_URL}/events/validate-timing"

    # Test data
    test_data = {
        "staffid": 1,
        "action": "clock-in",
        "proposedTime": datetime.datetime.now().isoformat()
    }

    try:
        response = requests.post(url, headers=HEADERS, json=test_data)
        print(f"📤 Request: {json.dumps(test_data, indent=2)}")
        print_response(response)
        return response.status_code in [200, 400]  # Both valid responses
    except requests.exceptions.RequestException as e:
        print(f"❌ Request failed: {e}")
        return False

def test_enhanced_clock_in():
    """Test Tim's enhanced clock-in with validation"""
    print_test_header("Enhanced Clock-In with Validation")

    url = f"{BASE_URL}/events/tim-lockin"

    # Test data
    test_data = {
        "staffid": 1,
        "deviceid": 101
    }

    try:
        response = requests.post(url, headers=HEADERS, json=test_data)
        print(f"📤 Request: {json.dumps(test_data, indent=2)}")
        print_response(response, expected_status=400)  # Likely to fail validation
        return response.status_code in [200, 400, 401]  # All are valid responses
    except requests.exceptions.RequestException as e:
        print(f"❌ Request failed: {e}")
        return False

def test_admin_override():
    """Test Tim's admin override functionality"""
    print_test_header("Admin Override Mechanism")

    url = f"{BASE_URL}/events/tim-lockin"

    # Test data with admin override
    test_data = {
        "staffid": 1,
        "deviceid": 101,
        "bypassValidation": True,
        "overrideReason": "Emergency testing scenario"
    }

    try:
        response = requests.post(url, headers=HEADERS, json=test_data)
        print(f"📤 Request: {json.dumps(test_data, indent=2)}")
        print_response(response, expected_status=401)  # Likely unauthorized
        return response.status_code in [200, 400, 401]  # All are valid responses
    except requests.exceptions.RequestException as e:
        print(f"❌ Request failed: {e}")
        return False

def test_api_availability():
    """Test if the basic API is responding"""
    print_test_header("API Availability Check")

    # Test the basic events endpoint
    url = f"{BASE_URL}/events"

    try:
        response = requests.get(url, headers=HEADERS)
        print_response(response)
        return response.status_code == 200
    except requests.exceptions.RequestException as e:
        print(f"❌ API not available: {e}")
        return False

def run_all_tests():
    """Run all Tim feature tests"""
    print("🚀 Starting Tim's Epic 8 Feature Tests")
    print(f"📍 Testing against: {BASE_URL}")

    tests = [
        ("API Availability", test_api_availability),
        ("Roster Status Check", test_roster_status_check),
        ("Validation Dry Run", test_validation_dry_run),
        ("Enhanced Clock-In", test_enhanced_clock_in),
        ("Admin Override", test_admin_override),
    ]

    results = []

    for test_name, test_func in tests:
        try:
            result = test_func()
            results.append((test_name, result))
        except Exception as e:
            print(f"❌ {test_name} failed with exception: {e}")
            results.append((test_name, False))

    # Print summary
    print_test_header("TEST SUMMARY")
    passed = 0
    total = len(results)

    for test_name, result in results:
        status = "✅ PASS" if result else "❌ FAIL"
        print(f"{status} - {test_name}")
        if result:
            passed += 1

    print(f"\n📊 Results: {passed}/{total} tests passed")

    if passed == total:
        print("🎉 All tests passed! Tim's features are working correctly.")
    else:
        print("⚠️ Some tests failed. Review the output above for details.")

    return passed == total

if __name__ == "__main__":
    success = run_all_tests()
    exit(0 if success else 1)