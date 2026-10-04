Feature: Role permissions
  As the owner of the information
  I want each role to do only what it is meant to do
  So that a veterinarian can follow animals without changing the herd

  Background:
    Given a rancher has a farm called "La Esperanza"
    And the farm has a corral called "Corral A"
    And the corral "Corral A" has the animal "COW-001"

  Scenario: A veterinarian can see the animals
    Given I am signed in as a "Veterinarian"
    When I request the list of animals
    Then the request succeeds
    And the animal "COW-001" appears in the animal list

  Scenario: A veterinarian cannot register animals
    Given I am signed in as a "Veterinarian"
    When I register an animal with the code "COW-009" in "Corral A"
    Then the request is rejected with status 403

  Scenario: A veterinarian cannot delete animals
    Given I am signed in as a "Veterinarian"
    When I delete the animal "COW-001"
    Then the request is rejected with status 403
