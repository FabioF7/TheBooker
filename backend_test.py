import requests
import sys
import json
from datetime import datetime, date, timedelta
from typing import Dict, Any, Optional

class TheBookerAPITester:
    def __init__(self, base_url="https://bookersaas.preview.emergentagent.com"):
        self.base_url = base_url
        self.tests_run = 0
        self.tests_passed = 0
        self.created_resources = {
            'tenants': [],
            'services': [],
            'providers': [],
            'appointments': []
        }

    def run_test(self, name: str, method: str, endpoint: str, expected_status: int, 
                 data: Optional[Dict] = None, params: Optional[Dict] = None) -> tuple[bool, Dict]:
        """Run a single API test"""
        url = f"{self.base_url}/{endpoint.lstrip('/')}"
        headers = {'Content-Type': 'application/json'}

        self.tests_run += 1
        print(f"\n🔍 Testing {name}...")
        print(f"   {method} {url}")
        
        try:
            if method == 'GET':
                response = requests.get(url, headers=headers, params=params)
            elif method == 'POST':
                response = requests.post(url, json=data, headers=headers)
            elif method == 'PUT':
                response = requests.put(url, json=data, headers=headers)
            elif method == 'DELETE':
                response = requests.delete(url, headers=headers)

            success = response.status_code == expected_status
            if success:
                self.tests_passed += 1
                print(f"✅ Passed - Status: {response.status_code}")
                try:
                    response_data = response.json() if response.content else {}
                except:
                    response_data = {}
            else:
                print(f"❌ Failed - Expected {expected_status}, got {response.status_code}")
                try:
                    error_data = response.json() if response.content else {}
                    print(f"   Error: {error_data}")
                except:
                    print(f"   Raw response: {response.text}")
                response_data = {}

            return success, response_data

        except Exception as e:
            print(f"❌ Failed - Error: {str(e)}")
            return False, {}

    def test_health_check(self) -> bool:
        """Test API health check endpoint"""
        success, _ = self.run_test(
            "Health Check",
            "GET",
            "api/health",
            200
        )
        return success

    def test_create_tenant(self, name: str, slug: str) -> Optional[Dict]:
        """Create a tenant and return tenant data"""
        tenant_data = {
            "name": name,
            "slug": slug,
            "timeZoneId": "America/New_York",
            "bufferMinutes": 15
        }
        
        success, response = self.run_test(
            f"Create Tenant '{name}'",
            "POST",
            "api/tenants",
            201,
            data=tenant_data
        )
        
        if success and response:
            self.created_resources['tenants'].append(response)
            return response
        return None

    def test_get_tenants(self) -> bool:
        """Get all tenants"""
        success, response = self.run_test(
            "Get All Tenants",
            "GET",
            "api/tenants",
            200
        )
        
        if success:
            print(f"   Found {len(response)} tenants")
        return success

    def test_get_tenant_by_slug(self, slug: str) -> Optional[Dict]:
        """Get tenant by slug"""
        success, response = self.run_test(
            f"Get Tenant by Slug '{slug}'",
            "GET",
            f"api/tenants/{slug}",
            200
        )
        return response if success else None

    def test_create_service(self, tenant_id: str, name: str, duration: int = 60, price: float = 50.0) -> Optional[Dict]:
        """Create a service for a tenant"""
        service_data = {
            "name": name,
            "durationMinutes": duration,
            "price": price,
            "currency": "USD",
            "description": f"Test service: {name}"
        }
        
        success, response = self.run_test(
            f"Create Service '{name}' for Tenant {tenant_id}",
            "POST",
            f"api/tenants/{tenant_id}/services",
            201,
            data=service_data
        )
        
        if success and response:
            self.created_resources['services'].append(response)
            return response
        return None

    def test_get_tenant_services(self, tenant_id: str) -> bool:
        """Get services for a tenant"""
        success, response = self.run_test(
            f"Get Services for Tenant {tenant_id}",
            "GET",
            f"api/tenants/{tenant_id}/services",
            200
        )
        
        if success:
            print(f"   Found {len(response)} services")
        return success

    def test_create_provider(self, tenant_id: str, name: str, email: str, service_ids: list = None) -> Optional[Dict]:
        """Create a provider for a tenant"""
        provider_data = {
            "name": name,
            "email": email,
            "serviceIds": service_ids or []
        }
        
        success, response = self.run_test(
            f"Create Provider '{name}' for Tenant {tenant_id}",
            "POST",
            f"api/tenants/{tenant_id}/providers",
            201,
            data=provider_data
        )
        
        if success and response:
            self.created_resources['providers'].append(response)
            return response
        return None

    def test_get_tenant_providers(self, tenant_id: str) -> bool:
        """Get providers for a tenant"""
        success, response = self.run_test(
            f"Get Providers for Tenant {tenant_id}",
            "GET",
            f"api/tenants/{tenant_id}/providers",
            200
        )
        
        if success:
            print(f"   Found {len(response)} providers")
        return success

    def test_get_availability(self, tenant_id: str, provider_id: str, service_id: str, date_str: str) -> Optional[Dict]:
        """Get availability for a provider on a specific date"""
        success, response = self.run_test(
            f"Get Availability for Provider {provider_id} on {date_str}",
            "GET",
            f"api/availability/{tenant_id}/{provider_id}/{service_id}/{date_str}",
            200,
            params={"slotInterval": 15}
        )
        return response if success else None

    def test_hold_slot(self, tenant_id: str, service_id: str, provider_id: str, 
                      date_str: str, start_time: str, session_id: str) -> Optional[Dict]:
        """Hold a time slot"""
        hold_data = {
            "tenantId": tenant_id,
            "serviceId": service_id,
            "providerId": provider_id,
            "date": date_str,
            "startTime": start_time,
            "sessionId": session_id
        }
        
        success, response = self.run_test(
            f"Hold Slot at {start_time} on {date_str}",
            "POST",
            "api/appointments/hold",
            200,
            data=hold_data
        )
        
        if success and response:
            self.created_resources['appointments'].append(response)
            return response
        return None

    def test_confirm_appointment(self, appointment_id: str, session_id: str) -> Optional[Dict]:
        """Confirm a held appointment"""
        confirm_data = {
            "sessionId": session_id,
            "customerName": "Test Customer",
            "customerEmail": "test@example.com",
            "customerPhone": "+1234567890",
            "notes": "Test appointment booking"
        }
        
        success, response = self.run_test(
            f"Confirm Appointment {appointment_id}",
            "POST",
            f"api/appointments/{appointment_id}/confirm",
            200,
            data=confirm_data
        )
        return response if success else None

    def run_full_booking_flow_test(self) -> bool:
        """Test the complete booking flow"""
        print("\n" + "="*60)
        print("🚀 TESTING COMPLETE BOOKING FLOW")
        print("="*60)
        
        # 1. Create tenant
        tenant = self.test_create_tenant("Test Salon", "test-salon")
        if not tenant:
            print("❌ Failed to create tenant - stopping flow test")
            return False
        
        tenant_id = tenant.get('id')
        if not tenant_id:
            print("❌ No tenant ID returned - stopping flow test")
            return False
        
        # 2. Create service
        service = self.test_create_service(tenant_id, "Haircut", 60, 50.0)
        if not service:
            print("❌ Failed to create service - stopping flow test")
            return False
        
        service_id = service.get('id')
        if not service_id:
            print("❌ No service ID returned - stopping flow test")
            return False
        
        # 3. Create provider
        provider = self.test_create_provider(tenant_id, "John Stylist", "john@testsalon.com", [service_id])
        if not provider:
            print("❌ Failed to create provider - stopping flow test")
            return False
        
        provider_id = provider.get('id')
        if not provider_id:
            print("❌ No provider ID returned - stopping flow test")
            return False
        
        # 4. Get availability for tomorrow
        tomorrow = (date.today() + timedelta(days=1)).strftime('%Y-%m-%d')
        availability = self.test_get_availability(tenant_id, provider_id, service_id, tomorrow)
        if not availability:
            print("❌ Failed to get availability - stopping flow test")
            return False
        
        # 5. Check if there are available slots
        if not availability.get('isOpen', False):
            print(f"⚠️  Provider not available on {tomorrow}: {availability.get('closedReason', 'Unknown reason')}")
            return False
        
        slots = availability.get('slots', [])
        available_slots = [slot for slot in slots if slot.get('isAvailable', False)]
        
        if not available_slots:
            print(f"⚠️  No available slots found for {tomorrow}")
            return False
        
        # 6. Hold first available slot
        first_slot = available_slots[0]
        session_id = f"test-session-{datetime.now().strftime('%Y%m%d%H%M%S')}"
        
        held_appointment = self.test_hold_slot(
            tenant_id, service_id, provider_id, 
            tomorrow, first_slot['startTime'], session_id
        )
        
        if not held_appointment:
            print("❌ Failed to hold slot - stopping flow test")
            return False
        
        appointment_id = held_appointment.get('appointmentId')
        if not appointment_id:
            print("❌ No appointment ID returned from hold - stopping flow test")
            return False
        
        # 7. Confirm appointment
        confirmed = self.test_confirm_appointment(appointment_id, session_id)
        if not confirmed:
            print("❌ Failed to confirm appointment - stopping flow test")
            return False
        
        print("\n✅ COMPLETE BOOKING FLOW SUCCESSFUL!")
        print(f"   Tenant: {tenant.get('name')} (ID: {tenant_id})")
        print(f"   Service: {service.get('name')} (ID: {service_id})")
        print(f"   Provider: {provider.get('name')} (ID: {provider_id})")
        print(f"   Appointment: {appointment_id} on {tomorrow} at {first_slot['startTime']}")
        
        return True

def main():
    print("🧪 Starting The Booker API Tests")
    print("="*50)
    
    tester = TheBookerAPITester()
    
    # Test individual endpoints
    print("\n📋 TESTING INDIVIDUAL ENDPOINTS")
    print("-" * 40)
    
    # Health check
    tester.test_health_check()
    
    # Tenant operations
    tester.test_get_tenants()
    
    # Test complete booking flow
    flow_success = tester.run_full_booking_flow_test()
    
    # Print final results
    print("\n" + "="*60)
    print("📊 FINAL TEST RESULTS")
    print("="*60)
    print(f"Tests run: {tester.tests_run}")
    print(f"Tests passed: {tester.tests_passed}")
    print(f"Success rate: {(tester.tests_passed/tester.tests_run*100):.1f}%")
    
    if flow_success:
        print("✅ Complete booking flow: PASSED")
    else:
        print("❌ Complete booking flow: FAILED")
    
    print(f"\n📝 Created Resources:")
    for resource_type, resources in tester.created_resources.items():
        if resources:
            print(f"   {resource_type}: {len(resources)} created")
    
    return 0 if tester.tests_passed == tester.tests_run and flow_success else 1

if __name__ == "__main__":
    sys.exit(main())