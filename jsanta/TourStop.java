/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package jsanta;

import java.sql.PreparedStatement;
import java.sql.SQLException;

/**
 *
 * @author arni
 */
public class TourStop {
    int gift_id;
    int group_id;
    double latitude;
    double longitude;
    double weight;
    double remaining_weigth;
    double tiredness;

    public TourStop(int gift_id,int group_id, double latitude, double longitude, double weight, double remaining_weigth, double tiredness) {
        this.gift_id = gift_id;
        this.group_id = group_id;
        this.latitude = latitude;
        this.longitude = longitude;
        this.weight = weight;
        this.remaining_weigth = remaining_weigth;
        this.tiredness = tiredness;
    }
    
    public int getGift_id() {
        return gift_id;
    }

    public void setGift_id(int gift_id) {
        this.gift_id = gift_id;
    }

    public double getLatitude() {
        return latitude;
    }

    public void setLatitude(double latitude) {
        this.latitude = latitude;
    }

    public double getLongitude() {
        return longitude;
    }

    public void setLongitude(double longitude) {
        this.longitude = longitude;
    }

    public double getWeight() {
        return weight;
    }

    public void setWeight(double weight) {
        this.weight = weight;
    }

    public double getRemaining_weigth() {
        return remaining_weigth;
    }

    public void setRemaining_weigth(double remaining_weigth) {
        this.remaining_weigth = remaining_weigth;
    }

    public double getTiredness() {
        return tiredness;
    }

    public void setTiredness(double tiredness) {
        this.tiredness = tiredness;
    }

    PreparedStatement setPreparedStatement(final PreparedStatement ps) throws SQLException {
        ps.setInt(1, gift_id);
        ps.setInt(2, group_id);
        ps.setDouble(3, latitude);
        ps.setDouble(4, longitude);
        ps.setDouble(5, weight);
        ps.setDouble(6, remaining_weigth);
        ps.setDouble(7, tiredness);
        return ps;
    }
    
}
