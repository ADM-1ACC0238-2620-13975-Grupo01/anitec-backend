Feature: Animal management
  As a rancher
  I want to register and organize my animals by farm and corral
  So that I always know what I have and in what condition

  Background:
    Given I am signed in as a "Rancher"
    And I have a farm called "La Esperanza"
    And the farm has a corral called "Corral A"

  Scenario: Register an animal in a corral
    When I register an animal with the code "COW-001" in "Corral A"
    Then the animal "COW-001" appears in the animal list

  Scenario: An animal must belong to a corral
    When I register an animal without a corral
    Then the request is rejected with status 400

  Scenario: Register several animals at once
    When I register 3 animals in bulk in "Corral A"
    Then the codes "CorralA-001, CorralA-002, CorralA-003" are assigned

  Scenario: Bulk registration is limited to 500 animals
    When I register 501 animals in bulk in "Corral A"
    Then the request is rejected with status 400

  Scenario: Mark several animals as sold
    Given the corral "Corral A" has the animal "COW-001"
    And the corral "Corral A" has the animal "COW-002"
    When I mark the animals "COW-001, COW-002" as "Vendido"
    Then the animals "COW-001, COW-002" have the status "Vendido"

  Scenario: Remove an animal
    Given the corral "Corral A" has the animal "COW-003"
    When I delete the animal "COW-003"
    Then the animal "COW-003" no longer appears in the animal list

  Scenario: Remove several animals at once
    Given the corral "Corral A" has the animal "COW-004"
    And the corral "Corral A" has the animal "COW-005"
    When I delete the animals "COW-004, COW-005"
    Then the animal "COW-004" no longer appears in the animal list
    And the animal "COW-005" no longer appears in the animal list
