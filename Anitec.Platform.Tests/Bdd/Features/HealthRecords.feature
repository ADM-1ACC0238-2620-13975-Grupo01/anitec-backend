Feature: Health records
  As a rancher or a veterinarian
  I want to keep the health history of each animal
  So that incidents, vaccines and treatments are never forgotten

  Background:
    Given a rancher has a farm called "La Esperanza"
    And the farm has a corral called "Corral A"
    And the corral "Corral A" has the animal "COW-001"

  Scenario: A rancher records a health incident
    Given I am signed in as a "Rancher"
    When I record a "Incidencia" health event for the animal "COW-001" with the description "Cojera en la pata trasera"
    Then the health event is created
    And the health record of the animal "COW-001" includes the description "Cojera en la pata trasera"

  Scenario: A veterinarian records a treatment
    Given I am signed in as a "Veterinarian"
    When I record a "Tratamiento" health event for the animal "COW-001" with the description "Antibiótico por 5 días"
    Then the health event is created
    And the health record of the animal "COW-001" includes the description "Antibiótico por 5 días"

  Scenario: Health records require a session
    Given I am a new visitor
    When I request the health records
    Then the request is rejected with status 401
