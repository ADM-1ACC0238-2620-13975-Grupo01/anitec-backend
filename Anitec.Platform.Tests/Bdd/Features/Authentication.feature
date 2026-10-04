Feature: Authentication
  As a rancher or a veterinarian
  I want to create an account and sign in
  So that I can work with my own information in AniTec

  Scenario: A new rancher signs up and signs in
    Given I am a new visitor
    When I sign up as a "Rancher"
    And I sign in with my credentials
    Then I receive a session token
    And my role is "Rancher"

  Scenario: A new veterinarian signs up and signs in
    Given I am a new visitor
    When I sign up as a "Veterinarian"
    And I sign in with my credentials
    Then I receive a session token
    And my role is "Veterinarian"

  Scenario: Signing up with an unknown role is rejected
    Given I am a new visitor
    When I sign up as a "Administrator"
    Then the request is rejected with status 400

  Scenario: Signing up twice with the same username is rejected
    Given I have an account as a "Rancher"
    When I sign up as a "Rancher"
    Then the request is rejected with status 409

  Scenario: Signing in with a wrong password is rejected
    Given I have an account as a "Rancher"
    When I sign in with the password "wrong-password"
    Then the request is rejected with status 400
    And I do not receive a session token

  Scenario: Protected information requires a session
    Given I am a new visitor
    When I request the list of animals
    Then the request is rejected with status 401

  Scenario: A signed-in user can request protected information
    Given I am signed in as a "Rancher"
    When I request the list of animals
    Then the request succeeds
