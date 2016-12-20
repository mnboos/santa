/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package jsanta;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.ArrayList;

/**
 *
 * @author arni
 */
public class Jsanta {

    private static double updateWeight(ArrayList<TourStop> stopList) {
        double totalWeight = 10; // Sledge is 10
        for (int i = stopList.size() - 1; i > -1; --i) {
            TourStop stop = stopList.get(i);
            totalWeight += stop.getWeight();
            stop.setRemaining_weigth(totalWeight);
        }
        return totalWeight;
    }

    private static double updateTiredness(ArrayList<TourStop> stopList) {
        double totalTiredness = 0;
        for (int i = 0; i < stopList.size(); i++) {
            TourStop currentStop = stopList.get(i);
            TourStop nextStop = stopList.get((i + 1) % stopList.size());
            currentStop.setTiredness(geodistance(currentStop, nextStop) * currentStop.getRemaining_weigth());
            totalTiredness += currentStop.getTiredness();
        }

        return totalTiredness;
    }

    private static double geodistance(TourStop a, TourStop b) {
        return Math.asin(Math.sqrt(
                Math.pow(Math.sin(Math.toRadians(b.getLatitude() - a.getLatitude()) / 2), 2)
                + Math.pow(Math.sin(Math.toRadians(b.getLongitude() - a.getLongitude()) / 2), 2)
                * Math.cos(Math.toRadians(a.getLatitude())) * Math.cos(Math.toRadians(b.getLatitude()))
        )) * 7926.3352;
    }

    public static void run2opt(ArrayList<ArrayList<TourStop>> groupList) {
        System.out.println("\nStarting 2opt");
        for (int group = 0; group < groupList.size(); ++group) {
            System.out.print(".");
            if ((group % 50) == 0) {
                System.out.print("\n");
            }
            System.out.flush();
            ArrayList<TourStop> stopList = groupList.get(group);
            double stopListTiredness = updateTiredness(stopList);
            ArrayList<TourStop> otherStopList = groupList.get((group + 1) % groupList.size());
            double otherStopListTiredness = updateTiredness(otherStopList);
            // start at 1, can not switch northpole
            for (int i = 1; i < stopList.size(); ++i) {
                for (int j = 1; j < otherStopList.size(); ++j) {
                    // Swap
                    TourStop tmp = stopList.get(i);
                    stopList.set(i, otherStopList.get(j));
                    otherStopList.set(j, tmp);

                    boolean worse = true;
                    // Check if valid
                    if (updateWeight(stopList) < 1000
                            && updateWeight(otherStopList) < 1000) {
                        double tmpTiredness = updateTiredness(stopList);
                        double tmpOtherTiredness = updateTiredness(otherStopList);
                        // Check if improved total tiredness
                        if (tmpTiredness + tmpOtherTiredness < stopListTiredness + otherStopListTiredness) {
                            worse = false;
                            stopListTiredness = tmpTiredness;
                            otherStopListTiredness = otherStopListTiredness;
                        }
                    }
                    if (worse) {
                        // Undo, if worse
                        tmp = stopList.get(i);
                        stopList.set(i, otherStopList.get(j));
                        otherStopList.set(j, tmp);
                    }
                }
            }
        }
    }

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        ArrayList<ArrayList<TourStop>> stops = new ArrayList<>();
        System.out.println(stops.size());
        Connection c = null;
        Statement stmt = null;
        try {
            Class.forName("org.postgresql.Driver");
            c = DriverManager
                    .getConnection("jdbc:postgresql://localhost:5432/santa",
                            "santa", "hax");
            c.setAutoCommit(false);
            System.out.println("Opened database successfully");
            stmt = c.createStatement();
            ResultSet rs = stmt.executeQuery("select * from gifts_solution order by group_id asc, remaining_weigth desc;");
            System.out.print("Loading Data");
            while (rs.next()) {
                int group = rs.getInt("group_id");
                if (stops.size() - 1 < group) {
                    stops.add(new ArrayList<>());
                    System.out.print(".");
                    if ((group % 50) == 0) {
                        System.out.print("\n");
                    }
                    System.out.flush();
                }
                stops.get(stops.size() - 1).add(new TourStop(
                        rs.getInt("gift_id"),
                        group,
                        rs.getDouble("latitude"),
                        rs.getDouble("longitude"),
                        rs.getDouble("weight"),
                        rs.getDouble("remaining_weigth"),
                        rs.getDouble("tiredness")));
            }

            double totalTiredness = 0;
            for (ArrayList<TourStop> group : stops) {
                for (TourStop stop : group) {
                    totalTiredness += stop.getTiredness();
                }
            }
            System.out.println("Tiredness before 2 opt");
            System.out.println(totalTiredness);

            run2opt(stops);
            String sql = "insert into gifts_2opt (gift_id, group_id, latitude, longitude, weight, remaining_weigth, tiredness) values (?,?,?,?,?,?,?)";
            PreparedStatement ps = c.prepareStatement(sql);
            
            ArrayList<String> queries = new ArrayList<>();
            totalTiredness = 0;
            for (ArrayList<TourStop> group : stops) {
                for (TourStop stop : group) {
                    totalTiredness += stop.getTiredness();
                    ps = stop.setPreparedStatement(ps);
                    ps.addBatch();
                }
            }
            System.out.println("Tiredness afater 2 opt");
            System.out.println(totalTiredness);
            ps.executeBatch();
            ps.close();
            c.commit();
            c.close();
        } catch (Exception e) {
            System.err.println(e.getClass().getName() + ": " + e.getMessage());
            e.printStackTrace();
            System.exit(0);
        }
    }

}
