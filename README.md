# QuoteOfTheDaySimulator

This simulator is paired with the quickstart located at https://github.com/jimmyca15/QuoteOfTheDayQuickStart.

After doing the referenced quickstart, ensure the app is running, open up a separate console and run the following commands.

```
git clone https://github.com/jimmyca15/QuoteOfTheDaySimulator.git

cd QuoteOfTheDaySimulator

dotnet build

dotnet run
```

### Example

Run the app by following the steps above.

![Running the application](./Images/Output.png)

When the application has finished it will show information of what it simulated.

![A completed run of the application](./Images/Completed.png)

### Example query

The following query can be run to see the results of the simulation

```
let userNamePrefix = "user";
// Total users
let total_users =
    customEvents
    | where name == "FeatureEvaluation"
    | where customDimensions.TargetingId startswith userNamePrefix
    | summarize TotalUsers = count() by Variant = tostring(customDimensions.Variant);
// Hearted users
let hearted_users =
    customEvents
    | where name == "FeatureEvaluation"
    | where customDimensions.TargetingId startswith userNamePrefix
    | extend TargetingId = tostring(customDimensions.TargetingId)
    | join kind=inner (
        customEvents
        | where name == "Like"
        | extend TargetingId = tostring(customDimensions.TargetingId)
    ) on TargetingId
    | summarize HeartedUsers = count() by Variant = tostring(customDimensions.Variant);
// Calculate the percentage of hearted users over total users
let combined_data =
    total_users
    | join kind=leftouter (hearted_users) on Variant
    | extend HeartedUsers = coalesce(HeartedUsers, 0)
    | extend PercentageHearted = strcat(round(HeartedUsers * 100.0 / TotalUsers, 1), "%")
    | project Variant, TotalUsers, HeartedUsers, PercentageHearted;
// Calculate the sum of total users and hearted users of all variants
let total_sum =
    combined_data
    | summarize Variant="All", TotalUsers = sum(TotalUsers), HeartedUsers = sum(HeartedUsers);
// Display the combined data along with the sum of total users and hearted users
combined_data
| union (total_sum)
```
![A query in Kusto showing the simulation data](./Images/KustoQuery.png)